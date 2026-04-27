namespace Adelaida.Evaluators;

public class CollectionEvaluator(Context ctx) : EvaluatorBase
{
	[EvaluatorMethod("clear")]
    public void Clear(string[] args)
    {
        if (ctx.RequiresCollection()) return;
        if (RequiresConfirmation()) return;

        ctx.Collection!.Clear();

        Console.WriteLine("collection cleared");
    }

    [EvaluatorMethod("add")]
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
    public void GetDocuments(string[] args)
    {
        if (ctx.RequiresCollection()) return;

        ctx.Collection!.GetDocuments().ForEach((x) => Console.WriteLine(x.Id));
    }

    [EvaluatorMethod("get-document")]
    public void GetDocument(string[] args)
    {
        if (ctx.RequiresCollection()) return;
        if (RequiresArguments(args)) return;

        ctx.Document = ctx.Collection!.GetDocument(args[0]);
    }

    [EvaluatorMethod("close-collection")]
    public void CloseCollection(string[] args)
    {
        if (ctx.RequiresCollection()) return;

        ctx.Collection = null;
    }
}