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

	public IEnumerator GetEnumerator() => Collections.GetEnumerator();

	public Alla(AllaOptions options)
	{
		Options = options;

		SerializerOptions = new()
		{
			WriteIndented = Options.PrettyPrint,
		};

		if (Options.EnumStrings)
		{
			SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
		}

		EnsureCreated();
		Load();
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
		if (hasOpenTransaction) throw new Exception("The transaction must be resolved before persisting the collection.");

		DeletePreviousPersistedFiles();

		PartitionStrategy strategy = Options.PartitionOptions.Strategy;

		if (strategy == PartitionStrategy.None)
		{
			PersistPartitionStrategyNone();
		}
		else if (strategy == PartitionStrategy.ByCollection)
		{
			PersistPartitionStrategyByCollection();
		}
		else
		{
			throw new Exception($"Not implemented partition strategy {Options.PartitionOptions.Strategy}");
		}
	}

	private void DeletePreviousPersistedFiles()
	{
		string datasourceFolder = Path.GetDirectoryName(Path.GetFullPath(Options.Datasource))
			?? Options.Datasource;

		Directory
			.EnumerateFiles(datasourceFolder, Options.GetPartitionFilename())
			.ToList().ForEach(File.Delete);
	}

	private void PersistPartitionStrategyNone() => File.WriteAllText(
		Options.Datasource,
		JsonSerializer.Serialize(
			Collections.Where((x) => x.Documents.Count > 0),
			SerializerOptions
		)
	);

	private void PersistPartitionStrategyByCollection()
	{
		List<Collection> collectionsWithDocuments = [.. Collections.Where((x) => x.Documents.Count > 0)];

		int partitionSize = Options.PartitionOptions.MaxSize;
		bool shouldPartitionCollections = partitionSize > 0;

		if (shouldPartitionCollections)
		{
			List<Collection> partitionedCollections = [];

			partitionedCollections = [.. collectionsWithDocuments
				.SelectMany((x) => x.Partition(partitionSize))
			];

			collectionsWithDocuments.Clear();
			collectionsWithDocuments.AddRange(partitionedCollections);
		}

		for (int i = 0; i < collectionsWithDocuments.Count; i++)
		{
			List<Collection> collectionSerializable = [collectionsWithDocuments[i]];
			File.WriteAllText(
				Options.GetPartitionFilename(i),
				JsonSerializer.Serialize(
					collectionSerializable,
					SerializerOptions
				)
			);
		}
	}

	private void EnsureCreated()
	{
		if (Options.Datasource == ":memory:") return;

		if (File.Exists(Options.Datasource)) return;
		
		Persist();
	}

	private void Load()
	{
		if (Options.PartitionOptions.Strategy == PartitionStrategy.None) LoadWithoutPartition();
		else LoadWithPartition();
	}

	private void LoadWithoutPartition() => Collections = JsonSerializer.Deserialize<List<Collection>>(
		File.ReadAllText(Options.Datasource)
	) ?? throw new Exception("Failed to deserialize datasource.");

	private void LoadWithPartition()
	{
		string datasourceFolder = Path.GetDirectoryName(Path.GetFullPath(Options.Datasource)) ?? Options.Datasource;

		IEnumerable<IGrouping<string, Collection>> partitions = Directory
			.EnumerateFiles(datasourceFolder, Options.GetPartitionFilename())
			.Select((x) => JsonSerializer.Deserialize<List<Collection>>(File.ReadAllText(x)))
			.Select((x) => x ?? throw new Exception("Failed to deserialize datasource partition."))
			.SelectMany((x) => x)
			.GroupBy((x) => x.Name);

		foreach (IGrouping<string, Collection> partition in partitions)
		{
			Collection? collection = Collections.Find((x) => x.Name == partition.Key);

			if (collection is not null) UpdateCollectionWithPartition(collection, partition);
			else AddCollectionInitialPartition(partition);
		}
	}

	private void UpdateCollectionWithPartition(Collection collection, IGrouping<string, Collection> partition) =>
		collection.AddRange(partition.SelectMany((x) => x.Documents.Select((x) => x.Fields)));

	private void AddCollectionInitialPartition(IGrouping<string, Collection> partition) => Collections.Add(new()
	{
		Name = partition.Key,
		Documents = [.. partition.SelectMany((x) => x.Documents)],
	});
}