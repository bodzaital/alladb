namespace AllaDb.Tests;

public class OptionsTests
{
	[TestCase("Data Source = database.json")]
	[TestCase("Data Source= database.json")]
	[TestCase("Data Source =database.json")]
	public void CanParse_WithOnlyDatasource(string connectionString)
	{
		AllaOptions options = AllaOptions.FromConnectionString(connectionString);

		Assert.That(options.Datasource, Is.EqualTo("database.json"));
	}

	[TestCase("DataSource = database.json")]
	[TestCase("data source = database.json")]
	[TestCase("datasource = database.json")]
	[TestCase("dataSource = database.json")]
	[TestCase("Datasource = database.json")]
	[TestCase("Data source = database.json")]
	public void Malformed_WithOnlyDatasource(string connectionString)
	{
		Assert.Throws<Exception>(() =>
		{
			AllaOptions.FromConnectionString(connectionString);
		});
	}

	[TestCase("Data Source = database.json, Pretty Print = true")]
	[TestCase("Data Source = database.json, Pretty Print= true")]
	[TestCase("Data Source = database.json, Pretty Print =true")]
	public void CanParse_WithPrettyPrint(string connectionString)
	{
		AllaOptions options = AllaOptions.FromConnectionString(connectionString);

		Assert.That(options.PrettyPrint, Is.True);
	}

	[TestCase("Data Source = database.json, Enum Strings = false")]
	[TestCase("Data Source = database.json, Enum Strings= false")]
	[TestCase("Data Source = database.json, Enum Strings =false")]
	public void CanParse_WithEnumStrings(string connectionString)
	{
		AllaOptions options = AllaOptions.FromConnectionString(connectionString);

		Assert.That(options.EnumStrings, Is.False);
	}

	[TestCase("Data Source = database.json, Enum Strings = false, Pretty Print = true")]
	public void CanParse_WithPrettyPrintAndEnumStrings(string connectionString)
	{
		AllaOptions options = AllaOptions.FromConnectionString(connectionString);

		using (Assert.EnterMultipleScope())
		{
			Assert.That(options.PrettyPrint, Is.True);
			Assert.That(options.EnumStrings, Is.False);
		}

	}

	[TestCase("Pretty Print = true")]
	[TestCase("Enum Strings = false")]
	[TestCase("Pretty Print = true, Enum Strings = false")]
	public void Malformed_WithoutDatasource(string connectionString)
	{
		Assert.Throws<Exception>(() =>
		{
			AllaOptions.FromConnectionString(connectionString);
		});
	}
}