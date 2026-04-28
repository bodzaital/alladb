namespace Adelaida.Evaluators;

public class AllaEvaluator(Context ctx) : EvaluatorBase
{
	[EvaluatorMethod("drop-database")]
    [EvaluatorDescription("Removes all collections from the database.")]
    public void DropDatabase(string[] args)
    {
        if (RequiresConfirmation()) return;

        ctx.Db.DropDatabase();
        ctx.IsDirty = true;
        Output.WriteLine(ConsoleColor.Yellow, "database dropped");
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
        ctx.IsDirty = true;
        Output.WriteLine(ConsoleColor.Yellow, "collection dropped");
    }

    [EvaluatorMethod("get-collections")]
    [EvaluatorDescription("Get all collections in the database.")]
    public void GetCollections(string[] args)
    {
        ctx.Db.GetCollections().ForEach((x) => Output.WriteLine($"{x.Name} ({x.GetDocuments().Count})"));
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
            ctx.IsDirty = false;
            Output.WriteLine(ConsoleColor.Green, "database saved");
        }
        catch (Exception e)
        {
            Output.WriteLine(ConsoleColor.Red, $"Error: {e.Message}");
        }
    }

    [EvaluatorMethod("status")]
    [EvaluatorDescription("Shows simple information regarding the current session.")]
    public void Status(string[] args)
    {
        if (ctx.Collection is null && ctx.Document is null && ctx.Transaction is null)
        {
            Output.WriteLine("Loaded database. Ready for commands.\n  List collections with \"get-collections\"");
        }

        if (ctx.Collection is not null) Output.WriteLine($"In a collection.\n  List documents with \"get-documents {ctx.Collection.Name}\"");
        if (ctx.Document is not null) Output.WriteLine($"Currently editing a document\n  List fields with \"get-fields {ctx.Document.Id}\"");
        if (ctx.Transaction is not null) Output.WriteLine($"In an active transaction.\n  Commit with \"commit\"\n  Roll back with \"roll-back\"");
        if (ctx.IsDirty) Output.WriteLine("There are unsaved changes. Save them with \"persist\"");
    }
}