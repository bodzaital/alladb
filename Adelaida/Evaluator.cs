using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using AllaDb;

namespace Adelaida;

public class Evaluator
{
    private static JsonSerializerOptions serializerOptions = new()
    {
        WriteIndented = true,
    };

    private readonly Dictionary<string, Action<string[]>> _evaluators;
    public Alla Db { get; init; }
    public Collection? Collection;
    public Document? Document;
    public bool IsLooping { get; set; } = true;

    public Evaluator(string connectionString)
    {
        Db = new(AllaOptions.FromConnectionString(connectionString));

        _evaluators = GetType()
            .GetMethods()
            .Where((x) => x.GetCustomAttribute<EvaluatorMethodAttribute>() is not null)
            .ToDictionary(
                (key) => key.GetCustomAttribute<EvaluatorMethodAttribute>()!.Name,
                (value) => value.CreateDelegate<Action<string[]>>(this)
            );
    }

    public void Evaluate(string name, string[] args)
    {
        if (_evaluators.Count == 0) throw new Exception("There are no evaluators.");

        if (!_evaluators.TryGetValue(name, out Action<string[]>? action))
        {
            Console.WriteLine($"No evaluator for {name}.");
            return;
        }

        action.Invoke(args);
    }

    public List<string> Evaluators()
    {
        return [.. GetType()
            .GetMethods()
            .Where((x) => x.GetCustomAttribute<EvaluatorMethodAttribute>() is not null)
            .Select((x) => x.GetCustomAttribute<EvaluatorMethodAttribute>()!.Name)
        ];
    }

    public Dictionary<string, string> GetEvaluators()
    {
        return GetType()
            .GetMethods()
            .Where((x) => x.GetCustomAttribute<EvaluatorMethodAttribute>() is not null)
            .Select((x) =>
            {
                string name = x.GetCustomAttribute<EvaluatorMethodAttribute>()!.Name;
                string? description = x.GetCustomAttribute<EvaluatorDescriptionAttribute>()?.Text;

                return new List<string>() {name, description ?? ""};
            }).ToDictionary((x) => x.First(), (x) => x.Last());
    }

    [EvaluatorMethod("help")]
    public void Help(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine(string.Join(", ", Evaluators()));
            return;
        }

        Dictionary<string, string> autocompletions = GetEvaluators()
			.Where((x) => x.Key.StartsWith(args.First()))
            .ToDictionary();

        if (autocompletions.Count > 1 && !autocompletions.ContainsKey(args.First()))
        {
            Console.WriteLine(string.Join(", ", autocompletions.Select((x) => x.Key)));
            return;
        }

        if (autocompletions.Count == 1 || autocompletions.ContainsKey(args.First()))
        {
            string line = autocompletions.First().Value != string.Empty
                ? $"{autocompletions.First().Key}: {autocompletions.First().Value}"
                : $"{autocompletions.First().Key}";
                
            Console.WriteLine(line);

            return;
        }

        Console.WriteLine("No function found.");
    }

    [EvaluatorMethod("exit")]
    [EvaluatorDescription("Persists the database and exits the CLI.")]
    public void Exit(string[] args)
    {
        IsLooping = false;
    }

    [EvaluatorMethod("drop-database")]
    public void DropDatabase(string[] args)
    {
        Db.DropDatabase();
    }

    [EvaluatorMethod("drop-collection")]
    public void DropCollection(string[] args)
    {
        if (RequiresArguments(args)) return;

        Db.DropCollection(args[0]);
    }

    [EvaluatorMethod("get-collections")]
    public void GetCollections(string[] args)
    {
        Db.GetCollections().ForEach((x) => Console.WriteLine(x.Name));
    }

    [EvaluatorMethod("get-collection")]
    public void GetCollection(string[] args)
    {
        if (RequiresArguments(args)) return;

        Collection = Db.GetCollection(args[0]);
        Document = null;
    }

    [EvaluatorMethod("clear")]
    public void Clear(string[] args)
    {
        if (RequiresCollection()) return;

        Collection!.Clear();
    }

    [EvaluatorMethod("add")]
    public void Add(string[] args)
    {
        if (RequiresCollection()) return;
        if (RequiresArguments(args)) return;

        Dictionary<string, object?> fields = args.ToList()
            .Select((x) => x.Split('='))
            .ToDictionary((key) => key[0], (val) => ParseFieldValueWithType<object?>(val[1]));

        Collection!.Add(fields);
    }

    [EvaluatorMethod("get-documents")]
    public void GetDocuments(string[] args)
    {
        if (RequiresCollection()) return;

        Collection!.GetDocuments().ForEach((x) => Console.WriteLine(x.Id));
    }

    [EvaluatorMethod("get-document")]
    public void GetDocument(string[] args)
    {
        if (RequiresCollection()) return;
        if (RequiresArguments(args)) return;

        Document = Collection!.GetDocument(args[0]);
    }

    [EvaluatorMethod("get-fields")]
    public void GetFields(string[] args)
    {
        if (RequiresCollection()) return;
        if (RequiresDocument()) return;

        Dictionary<string, object?> fields = Document!.GetFields();

        Console.WriteLine(JsonSerializer.Serialize(fields, serializerOptions));
    }

    [EvaluatorMethod("remove-field")]
    public void RemoveFields(string[] args)
    {
        if (RequiresCollection()) return;
        if (RequiresDocument()) return;

        if (RequiresArguments(args)) return;

        Document!.Remove(args[0]);
        Console.WriteLine("Removed.");
    }

    [EvaluatorMethod("set-fields")]
    public void SetFields(string[] args)
    {
        if (RequiresCollection()) return;
        if (RequiresDocument()) return;
        if (RequiresArguments(args)) return;

        Dictionary<string, object?> fields = args.ToList()
            .Select((x) => x.Split('='))
            .ToDictionary((key) => key[0], (val) => ParseFieldValueWithType<object?>(val[1]));

        fields.ToList().ForEach((x) => Document!.AddOrUpdate(x.Key, x.Value));
    }

    [EvaluatorMethod("persist")]
    public void Persist(string[] args)
    {
        Db.Persist();
    }

    private bool RequiresCollection()
    {
        if (Collection is null)
        {
            Console.WriteLine("This requires a collection.");
            return true;
        }

        return false;
    }

    private bool RequiresDocument()
    {
        if (Document is null)
        {
            Console.WriteLine("This requires a document.");
            return true;
        }

        return false;
    }

    private bool RequiresArguments(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("This requires arguments.");
            return true;
        }

        return false;
    }

    private static T? ParseFieldValueWithType<T>(string input)
    {
        if (input == "(null)") return default;

        Regex r = new(@"(?:\((?<type>\w+)\))?(?<value>.+)");
        Match m = r.Match(input);

        Group type = m.Groups["type"];
        Group value = m.Groups["value"];

        Type valueType = typeof(string);

        if (type.Success)
        {
            valueType = type.Value switch
            {
                "int" => typeof(int),
                "bool" => typeof(bool),
                _ => valueType,
            };
        }

        if (!value.Success)
        {
            Console.WriteLine($"Failed to parse value '{input}' with regex.");
            Console.WriteLine($"Parsed type was '{valueType}'.");
            throw new Exception("No field value.");
        }

        return (T)Convert.ChangeType(value.Value, valueType);
    }
}