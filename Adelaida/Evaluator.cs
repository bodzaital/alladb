using System.ComponentModel.DataAnnotations;
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

    private readonly List<EvaluatorInfo> _evaluatorInfos = [];
    public Alla Db { get; init; }
    public Collection? Collection;
    public Document? Document;
    public bool IsLooping { get; set; } = true;
    public Queue<string> History { get; set; } = [];

    public Evaluator(string connectionString)
    {
        Db = new(AllaOptions.FromConnectionString(connectionString));
        
        _evaluatorInfos = [.. GetType()
            .GetMethods()
            .Where((x) => x.GetCustomAttribute<EvaluatorMethodAttribute>() is not null)
            .Select((x) => new EvaluatorInfo(
                x.GetCustomAttribute<EvaluatorMethodAttribute>()!.Name,
                x.GetCustomAttribute<EvaluatorDescriptionAttribute>()?.Text ?? string.Empty,
                x.CreateDelegate<Action<string[]>>(this)
            ))
        ];
    }

    public void Evaluate(string name, string[] args)
    {
        EvaluatorInfo? evaluatorInfo = _evaluatorInfos.Find((x) => x.Name == name);
        if (evaluatorInfo is null)
        {
            Console.WriteLine($"No evaluator for {name}.");
            return;
        }

        evaluatorInfo.Action.Invoke(args);
    }

    public List<string> Evaluators() =>
        [.. _evaluatorInfos.Select((x) => x.Name)];

    public Dictionary<string, string> GetEvaluators() =>
        _evaluatorInfos.ToDictionary((x) => x.Name, (x) => x.Description);

    [EvaluatorMethod("history")]
    public void GetHistory(string[] args)
    {
        History.ToList().ForEach(Console.WriteLine);
    }

    public void PushHistory(string cmd)
    {
        History.Enqueue(cmd);
        if (History.Count > 10) History.Dequeue();
    }

    [EvaluatorMethod("help")]
    public void Help(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("List of functions:");
            Console.WriteLine(string.Join(", ", Evaluators()));
            Console.WriteLine("Type \"help [function]\" for more info.");
            return;
        }

        Dictionary<string, string> autocompletions = _evaluatorInfos
			.Where((x) => x.Name.StartsWith(args.First()))
            .ToDictionary((x) => x.Name, (x) => x.Description);

        KeyValuePair<string, string> foundFunction = autocompletions.FirstOrDefault((x) => x.Key == args.First());

        bool foundSpecificFunction = autocompletions.Count == 1;
        bool foundExactInAutocompletions = !foundFunction.Equals(default(KeyValuePair<string, string>));

        if (foundSpecificFunction || foundExactInAutocompletions)
        {
            KeyValuePair<string, string> function = foundSpecificFunction
                ? autocompletions.First()
                : foundFunction;

            bool hasDescription = function.Value != string.Empty;

            string text = hasDescription
                ? $"{function.Key}: {function.Value}"
                : function.Key;

            Console.WriteLine(text);
            return;
        }
        else if (autocompletions.Count > 0)
        {
            Console.WriteLine("List of functions:");
            Console.WriteLine(string.Join(", ", autocompletions.Select((x) => x.Key)));
            return;
        }

        Console.WriteLine("No function found.");
    }

    [EvaluatorMethod("exit")]
    [EvaluatorDescription("Exits the CLI.")]
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

    private static bool RequiresArguments(string[] args)
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