namespace AllaDb;

public class AllaOptions
{
	internal string DataSource { get; set; } = ":memory:";

	internal bool AreTransactionsRequired { get; set; } = false;

	internal bool IsPrettyPrint { get; set; } = false;

	internal bool IsEnumStrings { get; set; } = false;

	public AllaOptions AddDataSource(string dataSource)
	{
		DataSource = dataSource;
		return this;
	}

	public AllaOptions WithRequiredTransactions()
	{
		AreTransactionsRequired = true;
		return this;
	}

	public AllaOptions WithPrettyPrint()
	{
		IsPrettyPrint = true;
		return this;
	}

	public AllaOptions WithEnumStrings()
	{
		IsEnumStrings = true;
		return this;
	}
}