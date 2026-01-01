namespace AllaDb;

public class AllaOptions
{
	internal string DataSource { get; set; } = ":memory:";

	internal bool AreTransactionsRequired { get; set; } = false;

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
}