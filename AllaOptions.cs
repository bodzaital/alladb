namespace AllaDb;

public class AllaOptions
{
	internal string DataSource { get; set; } = ":memory:";

	public AllaOptions AddDataSource(string dataSource)
	{
		DataSource = dataSource;
		return this;
	}
}