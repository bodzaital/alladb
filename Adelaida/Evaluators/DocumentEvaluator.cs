using System.Text.Json;

namespace Adelaida.Evaluators;

public class DocumentEvaluator(Context ctx) : EvaluatorBase
{
	[EvaluatorMethod("get-fields")]
    public void GetFields(string[] args)
    {
        if (ctx.RequiresCollection()) return;
        if (ctx.RequiresDocument()) return;

        Dictionary<string, object?> fields = ctx.Document!.GetFields();

        Console.WriteLine(JsonSerializer.Serialize(fields, PrettySerializer));
    }

    [EvaluatorMethod("remove-field")]
    public void RemoveFields(string[] args)
    {
        if (ctx.RequiresCollection()) return;
        if (ctx.RequiresDocument()) return;

        if (RequiresArguments(args)) return;

        ctx.Document!.Remove(args[0]);
        Console.WriteLine("Removed.");
    }

    [EvaluatorMethod("set-fields")]
    public void SetFields(string[] args)
    {
        if (ctx.RequiresCollection()) return;
        if (ctx.RequiresDocument()) return;
        if (RequiresArguments(args)) return;

        Dictionary<string, object?> fields = args.ToList()
            .Select((x) => x.Split('='))
            .ToDictionary((key) => key[0], (val) => ParseFieldValueWithType<object?>(val[1]));

        fields.ToList().ForEach((x) => ctx.Document!.AddOrUpdate(x.Key, x.Value));
    }
}