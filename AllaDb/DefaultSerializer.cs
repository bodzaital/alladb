using System.Text.Json;

namespace AllaDb;

public class DefaultSerializer : IAllaSerializer
{
	public void EnsureCreated(Alla db)
	{
		if (File.Exists(db.Options.Datasource)) return;

		Persist(db);
	}

	public List<Collection> Load(Alla db)
	{
		return JsonSerializer.Deserialize<List<Collection>>(
			File.ReadAllText(db.Options.Datasource) 
		) ?? throw new Exception("Failed to deserialize datasource.");
	}

	public void Persist(Alla db)
	{
		File.WriteAllText(
			db.Options.Datasource,
			JsonSerializer.Serialize(
				db.Collections.Where((x) => x.Documents.Count > 0),
				db.SerializerOptions
			)
		);
	}
}