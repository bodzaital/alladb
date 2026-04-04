using System.Collections;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AllaDb;

public class Alla
{
	[JsonIgnore]
	public AllaOptions Options { get; set; }

	[JsonIgnore]
	public JsonSerializerOptions SerializerOptions { get; set; }

	public List<Collection> Collections { get; set; } = [];

	public IAllaSerializer Serializer { get; set; }

	public IEnumerator GetEnumerator() => Collections.GetEnumerator();

	public Alla(AllaOptions options, IAllaSerializer? serializer = null)
	{
		Options = options;
		Serializer = serializer ?? new DefaultSerializer();

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

	public void DropDatabase() => Collections.Clear();

	public void DropCollection(string name) => Collections.RemoveAll((x) => x.Name == name);

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

	public void Persist()
	{
		if (Options.Datasource == ":memory:") throw new Exception("Database cannot be persisted as it is in-memory only.");

		bool hasOpenTransaction = Collections.Any((x) => x.Transactions.Count > 0);
		if (hasOpenTransaction) throw new Exception("Any open transactions must be resolved before persisting the collection.");

		Serializer.Persist(this);
	}

	private void EnsureCreated()
	{
		if (Options.Datasource == ":memory:") return;
		
		Serializer.EnsureCreated(this);
	}

	private List<Collection> Load()
	{
		if (Options.Datasource == ":memory:") return [];

		return Serializer.Load(this);
	}
}