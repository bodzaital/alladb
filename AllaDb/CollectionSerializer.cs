using System.Text.Json;

namespace AllaDb;

/// <summary>A serializer that partitions by collections.</summary>
public class CollectionSerializer : IAllaSerializer
{
	private static readonly string DatasourceFile = "database.[collection].json";

	private readonly List<string> _loadedCollectionNames = [];

	/// <inheritdoc />
	public void EnsureCreated(Alla db)
	{
		return;
	}

	/// <inheritdoc />
	public List<Collection> Load(Alla db)
	{
		List<string> datasourceFileNames = GetDatasourceFileNames(GetFullDatasource(db.Options.Datasource));

		if (datasourceFileNames.Count == 0) return [];

		_loadedCollectionNames.Clear();

		return [.. datasourceFileNames.Select((x) =>
			ReadAndDeserialize(db, x)
		)];
	}

	/// <inheritdoc />
	public void Persist(Alla db)
	{
		DeleteAnyRemovedCollectionFiles(db);

		db.Collections.Where((x) => x.Documents.Count > 0).ToList()
			.ForEach((collection) => File.WriteAllText(
				GetFullDatasource(db.Options.Datasource).Replace("[collection]", collection.Name),
				JsonSerializer.Serialize(
					collection,
					db.SerializerOptions
				)
			));
	}

	private void DeleteAnyRemovedCollectionFiles(Alla db)
	{
		List<string> datasourceFileNames = GetDatasourceFileNames(GetFullDatasource(db.Options.Datasource));

		datasourceFileNames.ForEach((fileName) =>
		{
			_loadedCollectionNames.ForEach((loadedCollectionName) =>
			{
				if (db.Collections.Find((x) => x.Name == loadedCollectionName) is null)
				{
					File.Delete(fileName);
				}
			});
		});
	}

	private Collection ReadAndDeserialize(Alla db, string fileName)
	{
		Collection? collection = JsonSerializer.Deserialize<Collection>(
			File.ReadAllText(fileName),
			db.SerializerOptions
		) ?? throw new Exception();

		_loadedCollectionNames.Add(collection.Name);

		return collection;
	}

	private static List<string> GetDatasourceFileNames(string datasource)
	{
		string datasourceAbsolutePath = Path.GetFullPath(datasource);

		string datasourceDirectory = Path.GetDirectoryName(datasourceAbsolutePath)
			?? datasourceAbsolutePath;

		string datasourcePattern = Path.GetFileName(datasourceAbsolutePath)
			.Replace("[collection]", "*");

		return [.. Directory
			.GetFiles(datasourceDirectory, datasourcePattern)
		];
	}

	private static string GetFullDatasource(string datasource) =>
		Path.Join(datasource, DatasourceFile);
}