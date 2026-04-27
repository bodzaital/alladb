namespace Adelaida.Evaluators;

public class CollectionEvaluator(Context ctx) : EvaluatorBase
{
	[EvaluatorMethod("clear")]
    [EvaluatorDescription("Removes all documents from the collection.")]
    public void Clear(string[] args)
    {
        if (ctx.RequiresCollection()) return;
        if (RequiresConfirmation()) return;

        ctx.Collection!.Clear();

        Console.WriteLine("collection cleared");
    }

    [EvaluatorMethod("add")]
    [EvaluatorDescription("Adds a new document with the specified fields to the end of the collection.")]
    public void AddDocument(string[] args)
    {
        if (ctx.RequiresCollection()) return;
        if (RequiresArguments(args)) return;

        Dictionary<string, object?> fields = args.ToList()
            .Select((x) => x.Split('='))
            .ToDictionary((key) => key[0], (val) => ParseFieldValueWithType<object?>(val[1]));

        ctx.Collection!.Add(fields);

        Console.WriteLine("document added");
    }

    [EvaluatorMethod("remove")]
    [EvaluatorDescription("Removes the specific document from the collection.")]
    public void RemoveDocument(string[] args)
    {
        if (ctx.RequiresCollection()) return;
        if (ctx.RequiresDocument()) return;
        if (RequiresArguments(args)) return;
        if (RequiresConfirmation()) return;

        ctx.Collection!.Remove(ctx.Document!);
        ctx.Document = null;

        Console.WriteLine("document removed");
    }

    [EvaluatorMethod("get-documents")]
    [EvaluatorDescription("Get all documents of the collection.")]
    public void GetDocuments(string[] args)
    {
        if (ctx.RequiresCollection()) return;

        ctx.Collection!.GetDocuments().ForEach((x) => Console.WriteLine(x.Id));
    }

    [EvaluatorMethod("get-document")]
    [EvaluatorDescription("Gets the document associated with the specified ID.")]
    public void GetDocument(string[] args)
    {
        if (ctx.RequiresCollection()) return;
        if (RequiresArguments(args)) return;

        ctx.Document = ctx.Collection!.GetDocument(args[0]);
    }

    [EvaluatorMethod("close-collection")]
    [EvaluatorDescription("Releases the current collection from memory.")]
    public void CloseCollection(string[] args)
    {
        if (ctx.RequiresCollection()) return;

        ctx.Collection = null;
    }
}