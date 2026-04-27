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
    [EvaluatorDescription("args: [name], Removes the collection whose name matches the specified name.")]
    public void DropCollection(string[] args)
    {
        if (RequiresArguments(args)) return;
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
    [EvaluatorDescription("args: [name], Creates a collection in the database if the name does not already exist, or gets the collection in the database if the name already exists.")]
    public void GetCollection(string[] args)
    {
        if (RequiresArguments(args)) return;

        ctx.Collection = ctx.Db.GetCollection(args[0]);
        ctx.Document = null;
    }

    [EvaluatorMethod("persist")]
    [EvaluatorDescription("Serializes the database based on the connection string and the serializer.")]
    public void Persist(string[] args)
    {
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
}