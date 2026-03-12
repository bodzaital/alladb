using System.Text.Json;

namespace AllaDb;

public class AllaOptions(string datasource)
{
	public string Datasource { get; set; } = datasource;

	public bool PrettyPrint { get; set; } = false;

	public bool EnumStrings { get; set; } = true;

	public PartitionOptions PartitionOptions { get; set; } = new();

	public static AllaOptions FromConnectionString(string connectionString) => JsonSerializer.Deserialize<AllaOptions>(connectionString)
		?? throw new Exception("Failed to parse connection string.");

	public string GetPartitionFilename(int? id = null)
	{
		string dotExt = Path.GetExtension(Datasource);

		return id is null
			? Datasource.Replace(dotExt, $".partition*{dotExt}")
			: Datasource.Replace(dotExt, $".partition{id}{dotExt}");
	}
}

public class PartitionOptions
{
	public PartitionStrategy Strategy { get; set; } = PartitionStrategy.None;

	public int MaxSize { get; set; } = 0;
}

public enum PartitionStrategy
{
	None,
	ByCollection,
}