using System.Net.WebSockets;
using System.Reflection;
using System.Text.Json;

namespace AllaDb;

public class AllaOptions(string datasource)
{
	public string Datasource { get; set; } = datasource;

	public bool PrettyPrint { get; set; } = false;

	public bool EnumStrings { get; set; } = true;

	public PartitionOptions PartitionOptions { get; set; } = new();

	public static AllaOptions FromConnectionString(string connectionString)
	{
		StringSplitOptions splitOptions = StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries;

		Dictionary<string, string> settings = connectionString
			.Split(',', splitOptions)
			.Select((x) => x.Split('=', splitOptions))
			.ToDictionary((x) => x[0], (x) => x[1]);
		
		if (!settings.TryGetValue("Data Source", out string? datasource)) throw new Exception(
			"Failed to parse connection string: missing required key \"Data Source\"."
		);

		AllaOptions options = new(datasource)
		{
			PartitionOptions = PartitionOptions.FromConnectionString(settings)
		};

		if (settings.TryGetValue("Pretty Print", out string? isPrettyPrint)) options.PrettyPrint = bool.Parse(isPrettyPrint);
		if (settings.TryGetValue("Enum Strings", out string? isEnumStrings)) options.PrettyPrint = bool.Parse(isEnumStrings);

		return options;
	}

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

	public static PartitionOptions FromConnectionString(Dictionary<string, string> settings)
	{
		bool hasPartitionOptions = settings.ToList()
			.Any((x) => x.Key.Contains("Partition Options"));

		if (!hasPartitionOptions) return new();

		PartitionOptions partitionOptions = new();

		if (settings.TryGetValue("Partition Options: Strategy", out string? strategy))
		{
			partitionOptions.Strategy = Enum.Parse<PartitionStrategy>(strategy);
		}

		if (settings.TryGetValue("Partition Options: Max Size", out string? maxSize))
		{
			partitionOptions.MaxSize = int.Parse(maxSize);
		}

		return partitionOptions;
	}
}

public enum PartitionStrategy
{
	None,
	ByCollection,
}