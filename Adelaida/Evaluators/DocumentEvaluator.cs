using System.Text.Json;

namespace Adelaida.Evaluators;

public class DocumentEvaluator(Context ctx) : EvaluatorBase
{
	[EvaluatorMethod("get-fields")]
    [EvaluatorDescription("Get all fields of the document.")]
    public void GetFields(string[] args)
    {
        if (ctx.RequiresCollection()) return;
        if (ctx.RequiresDocument()) return;

        Dictionary<string, object?> fields = ctx.Document!.GetFields();

        Console.WriteLine(JsonSerializer.Serialize(fields, PrettySerializer));
    }

    [EvaluatorMethod("remove-fields")]
    [EvaluatorDescription("Removes the value with the specified key from the fields.")]
    public void RemoveField(string[] args)
    {
        Dictionary<string, string> requiredArgs = new()
        {
            { "key", "key of the field to remove" },
        };

        if (ctx.RequiresCollection()) return;
        if (ctx.RequiresDocument()) return;
        if (RequiresArguments(args, requiredArgs)) return;
        if (RequiresConfirmation()) return;

        ctx.Document!.Remove(args[0]);
        Console.WriteLine("fields removed");
    }

    [EvaluatorMethod("set-fields")]
    [EvaluatorDescription("Adds a field to the document if the key does not already exist, or updates a field in the document if the key already exists.")]
    public void SetFields(string[] args)
    {
        Dictionary<string, string> requiredArgs = new()
        {
            { "fields", "list of key=value, optionally enclosed in \" and the value typed with (T) prefix for primitive types" },
        };

        if (ctx.RequiresCollection()) return;
        if (ctx.RequiresDocument()) return;
        if (RequiresArguments(args, requiredArgs)) return;

        Dictionary<string, object?> fields = args.ToList()
            .Select((x) => x.Split('='))
            .ToDictionary((key) => key[0], (val) => ParseFieldValueWithType<object?>(val[1]));

        fields.ToList().ForEach((x) => ctx.Document!.AddOrUpdate(x.Key, x.Value));

        Output.WriteLine(ConsoleColor.Green, $"{fields.Count} fields set");
    }

    [EvaluatorMethod("close-document")]
    [EvaluatorDescription("Releases the current document from memory.")]
    public void CloseDocument(string[] args)
    {
        if (ctx.RequiresCollection()) return;
        if (ctx.RequiresDocument()) return;

        ctx.Document = null;
    }
}