namespace AllaDb;

public class AllaOptions
{
	internal string DataSource { get; set; } = ":memory:";

	internal bool AreTransactionsRequired { get; set; } = false;

	internal bool IsPrettyPrint { get; set; } = false;

	internal bool IsEnumStrings { get; set; } = false;

	/// <summary>Adds a data source file to the database.</summary>
	/// <param name="dataSource">Path to the data source file.</param>
	public AllaOptions AddDataSource(string dataSource)
	{
		DataSource = dataSource;
		return this;
	}

	/// <summary>Requires access to the database to use transactions.</summary>
	public AllaOptions WithRequiredTransactions()
	{
		AreTransactionsRequired = true;
		return this;
	}

	/// <summary>Specifies serialization to use indentations.</summary>
	public AllaOptions WithPrettyPrint()
	{
		IsPrettyPrint = true;
		return this;
	}

	/// <summary>Specifies serialization to use string constants for Enum types.</summary>
	public AllaOptions WithEnumStrings()
	{
		IsEnumStrings = true;
		return this;
	}
}