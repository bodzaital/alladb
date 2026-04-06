using System.Collections;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AllaDb;

/// <summary>A lightweight, limited application database storing key-value pairs.</summary>
public class Alla
{
	private readonly IAllaSerializer _serializer;

	internal AllaOptions Options { get; set; }

	internal JsonSerializerOptions SerializerOptions { get; set; }

	internal List<Collection> Collections { get; set; } = [];

	/// <summary>Creates a new instance of Alla Db.</summary>
	/// <param name="serializer">The serializer to use when persisting the database. If null, uses the default single-file serializer.</param>
	public Alla(AllaOptions options, IAllaSerializer? serializer = null)
	{
		Options = options;
		_serializer = serializer ?? new DefaultSerializer();

		SerializerOptions = new()
		{
			WriteIndented = Options.PrettyPrint,
		};

		if (Options.EnumStrings)
		{
			SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
		}

		EnsureCreated();
		Collections = Load();
	}

	/// <summary>Exposes the enumerator of the underlying list of collections.</summary>
	public IEnumerator GetEnumerator() => Collections.GetEnumerator();

	/// <summary>Deletes the database and all collections.</summary>
	public void DropDatabase() => Collections.Clear();

	/// <summary>Deletes a collection and all documents.</summary>
	public void DropCollection(string name) => Collections.RemoveAll((x) => x.Name == name);

	/// <summary>Creates a collection with the specified name or returns one if already exists.</summary>
	public Collection GetCollection(string name)
	{
		Collection? collection = Collections.Find((x) => x.Name == name);
		
		if (collection is null)
		{
			collection = new() { Name = name };
			Collections.Add(collection);
		}
		
		return collection;
	}

	/// <summary>Serializes the database based on the connection string and the serializer. Throws an exception for in-memory databases.</summary>
	public void Persist()
	{
		if (Options.Datasource == ":memory:") 
		{
			throw new Exception("Database cannot be persisted as it is in-memory only.");
		}

		bool hasOpenTransaction = Collections.Any((x) => x.Transactions.Count > 0);

		if (hasOpenTransaction)
		{
			throw new Exception("Any open transactions must be resolved before persisting the collection.");
		}

		_serializer.Persist(this);
	}

	private void EnsureCreated()
	{
		if (Options.Datasource == ":memory:") return;
		
		_serializer.EnsureCreated(this);
	}

	private List<Collection> Load()
	{
		if (Options.Datasource == ":memory:") return [];

		List<Collection> loaded = _serializer.Load(this);
		loaded.ForEach(SetCollectionReference);

		return loaded;
	}

	private void SetCollectionReference(Collection collection)
	{
		collection.Documents.ForEach((document) =>
		{
			document.Collection = collection;
		});
	}
}