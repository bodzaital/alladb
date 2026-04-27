using AllaDb;

namespace Adelaida;

public class Context(string connectionString)
{
	public Alla Db { get; init; } = new(AllaOptions.FromConnectionString(connectionString));

	public Collection? Collection { get; set; }
	
	public Document? Document { get; set; }

	public string GetHandle()
	{
		string editing = Document?.Id is not null
			? $" editing {Document.Id}"
			: string.Empty;

		return $"{Collection?.Name ?? "no collection"}{editing}";
	}

	public bool RequiresCollection()
	{
		if (Collection is null)
		{
			Console.WriteLine("This function requires a collection.");
			return true;
		}

		return false;
	}

    public bool RequiresDocument()
    {
        if (Document is null)
        {
            Console.WriteLine("This function requires a document.");
            return true;
        }

        return false;
    }
}