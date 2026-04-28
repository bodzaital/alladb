using AllaDb;

namespace Adelaida;

public class Context(string connectionString)
{
	public Alla Db { get; init; } = new(AllaOptions.FromConnectionString(connectionString));

	public Collection? Collection { get; set; }
	
	public Document? Document { get; set; }

	public Transaction? Transaction { get; set; }

    public bool IsDirty { get; set; }

	public string GetHandle()
	{
		string editing = Document?.Id is not null
			? $" editing {Document.Id}"
			: string.Empty;

		string dirtyFlag = IsDirty
			? "*"
			: string.Empty;
        
        string[] parentheses = Transaction is not null
            ? [ "[", "]" ]
            : [ "(", ")" ];

		return $"{parentheses.First()}{dirtyFlag}{Collection?.Name ?? "no collection"}{editing}{parentheses.Last()}";
	}

	public bool RequiresCollection()
	{
		if (Collection is null)
		{
			Output.WriteLine(ConsoleColor.Red, "This function requires a collection.");
			return true;
		}

		return false;
	}

    public bool RequiresDocument()
    {
        if (Document is null)
        {
            Output.WriteLine(ConsoleColor.Red, "This function requires a document.");
            return true;
        }

        return false;
    }

    public bool RequiresTransaction()
    {
        if (Transaction is null)
        {
            Output.WriteLine(ConsoleColor.Red, "This function requires a transaction.");
            return true;
        }

        return false;
    }

    public bool RequiresNoTransaction()
    {
        if (Transaction is not null)
        {
            Output.WriteLine(ConsoleColor.Red, "This function requires no transaction.");
            return true;
        }

        return false;
    }
}