namespace Adelaida.Evaluators;

public class AllaEvaluator(Context ctx) : EvaluatorBase
{
	[EvaluatorMethod("drop-database")]
    [EvaluatorDescription("Removes all collections from the database.")]
    public void DropDatabase(string[] args)
    {
        if (RequiresConfirmation()) return;

        ctx.Db.DropDatabase();
        Console.WriteLine("database dropped");
    }

    [EvaluatorMethod("drop-collection")]
    [EvaluatorDescription("Removes the collection whose name matches the specified name.")]
    public void DropCollection(string[] args)
    {
        Dictionary<string, string> requiredArgs = new()
        {
            { "name", "name of the collection" },
        };

        if (RequiresArguments(args, requiredArgs)) return;
        if (RequiresConfirmation()) return;

        ctx.Db.DropCollection(args[0]);
        Console.WriteLine("collection dropped");
    }

    [EvaluatorMethod("get-collections")]
    [EvaluatorDescription("Get all collections in the database.")]
    public void GetCollections(string[] args)
    {
        ctx.Db.GetCollections().ForEach((x) => Console.WriteLine($"{x.Name} ({x.GetDocuments().Count})"));
    }

    [EvaluatorMethod("get-collection")]
    [EvaluatorDescription("Creates a collection in the database if the name does not already exist, or gets the collection in the database if the name already exists.")]
    public void GetCollection(string[] args)
    {
        Dictionary<string, string> requiredArgs = new()
        {
            { "name", "name of the collection" },
        };

        if (RequiresArguments(args, requiredArgs)) return;

        ctx.Collection = ctx.Db.GetCollection(args[0]);
        ctx.Document = null;
    }

    [EvaluatorMethod("persist")]
    [EvaluatorDescription("Serializes the database based on the connection string and the serializer.")]
    public void Persist(string[] args)
    {
        if (ctx.RequiresNoTransaction()) return;
        
        try
        {
            ctx.Db.Persist();
            Console.WriteLine("database saved");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error: {e.Message}");
        }
    }

    [EvaluatorMethod("status")]
    [EvaluatorDescription("Shows simple information regarding the current session.")]
    public void Status(string[] args)
    {
        if (ctx.Collection is null && ctx.Document is null && ctx.Transaction is null)
        {
            Console.WriteLine("Loaded database. Ready for commands.\n  List collections with \"get-collections\"");
        }

        if (ctx.Collection is not null) Console.WriteLine($"In a collection.\n  List documents with \"get-documents {ctx.Collection.Name}\"");
        if (ctx.Document is not null) Console.WriteLine($"Currently editing a document\n  List fields with \"get-fields {ctx.Document.Id}\"");
        if (ctx.Transaction is not null) Console.WriteLine($"In an active transaction.\n  Commit with \"commit\"\n  Roll back with \"roll-back\"");
    }
}