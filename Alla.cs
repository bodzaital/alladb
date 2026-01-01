using System.Text.Json;
using System.Text.Json.Serialization;
using AllaDb.Exceptions;
using Microsoft.Extensions.Options;

namespace AllaDb;

public class Alla : IAlla
{
	private readonly AllaOptions _options;

	[JsonInclude]
	private readonly List<Collection> Collections = [];

	public Alla(IOptions<AllaOptions> options)
	{
		ArgumentNullException.ThrowIfNull(options);
		_options = options.Value;

		EnsureCreated();
		Collections = Load();
	}

	public void DropDatabase() => Collections.Clear();

	public void DropCollection(string collectionName) => Collections
		.RemoveAll((x) => x.Name == collectionName);

	public Collection GetCollection(string collectionName)
	{
		Collection? collection = Collections.Find((x) => x.Name == collectionName);

		if (collection is null)
		{
			collection = new(collectionName);
			Collections.Add(collection);
		}

		return collection;
	}

	public void Persist()
	{
		if (_options.DataSource == ":memory:") throw new InvalidOperationException(
			"Database cannot be persisted because it is in-memory only."
		);

		if (Collections.Any((x) => x.Transaction is not null)) throw new UnresolvedTransactionException(
			"The transaction must be resolved before persisting the collection."
		);
		
		File.WriteAllText(_options.DataSource, JsonSerializer.Serialize(Collections.Where((x) => x.Documents.Count > 0)));
	}

	private List<Collection> Load() => JsonSerializer.Deserialize<List<Collection>>(File.ReadAllText(_options.DataSource))
		?? throw new IllegalDeserializationException();

	private void EnsureCreated()
	{
		if (File.Exists(_options.DataSource)) return;
		Persist();
	}
}