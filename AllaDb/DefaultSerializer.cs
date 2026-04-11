using System.Text.Json;

namespace AllaDb;

/// <summary>The default, single-file serializer used by the database when persisting.</summary>
public class DefaultSerializer : IAllaSerializer
{
	private static readonly string DatasourceFile = "database.json";

	/// <inheritdoc />
	public void EnsureCreated(Alla db)
	{
		if (File.Exists(GetFullDatasource(db.Options.Datasource))) return;

		Persist(db);
	}

	/// <inheritdoc />
	public List<Collection> Load(Alla db)
	{
		return JsonSerializer.Deserialize<List<Collection>>(
			File.ReadAllText(GetFullDatasource(db.Options.Datasource)) 
		) ?? throw new Exception("Failed to deserialize datasource.");
	}

	/// <inheritdoc />
	public void Persist(Alla db)
	{
		File.WriteAllText(
			GetFullDatasource(db.Options.Datasource),
			JsonSerializer.Serialize(
				db.Collections.Where((x) => x.Documents.Count > 0),
				db.SerializerOptions
			)
		);
	}

	private static string GetFullDatasource(string datasource) =>
		Path.Join(datasource, DatasourceFile);
}