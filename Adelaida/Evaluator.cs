using System.Reflection;
using AllaDb;

namespace Adelaida;

public class Evaluator
{
    private readonly Dictionary<string, Action<string[]>> _evaluators;
    public Alla Db { get; init; }
    public Collection? Collection;
    public Document? Document;
    public bool IsLooping { get; set; } = true;

    public Evaluator(string connectionString)
    {
        Db = new(AllaOptions.FromConnectionString(connectionString));

        _evaluators =  GetType()
            .GetMethods()
            .Where((x) => x.GetCustomAttribute<EvaluatorAttribute>() is not null)
            .ToDictionary(
                (key) => key.GetCustomAttribute<EvaluatorAttribute>()!.Name,
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

    [Evaluator("exit")]
    public void Exit(string[] args)
    {
        IsLooping = false;
    }

    [Evaluator("drop-database")]
    public void DropDatabase(string[] args)
    {
        Db.DropDatabase();
    }

    [Evaluator("drop-collection")]
    public void DropCollection(string[] args)
    {
        Db.DropCollection(args[0]);
    }

    [Evaluator("get-collections")]
    public void GetCollections(string[] args)
    {
        Db.GetCollections().ForEach((x) => Console.WriteLine(x.Name));
    }

    [Evaluator("get-collection")]
    public void GetCollection(string[] args)
    {
        Collection = Db.GetCollection(args[0]);
        Document = null;
    }

    [Evaluator("clear")]
    public void Clear(string[] args)
    {
        Collection!.Clear();
    }

    [Evaluator("add")]
    public void Add(string[] args)
    {
        Dictionary<string, object?> fields = args.ToList()
            .Select((x) => x.Split('='))
            .ToDictionary((key) => key[0], (val) => (object?)val[1]);

        Collection!.Add(fields);
    }

    [Evaluator("get-documents")]
    public void GetDocuments(string[] args)
    {
        Collection?.GetDocuments().ForEach((x) => Console.WriteLine(x.Id));
    }

    [Evaluator("get-document")]
    public void GetDocument(string[] args)
    {
        Document = Collection?.GetDocument(args[0]);
    }
}