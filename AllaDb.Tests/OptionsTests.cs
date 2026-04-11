namespace AllaDb.Tests;

public class OptionsTests
{
	[TestCase("Data Source = /test/datasource/folder")]
	[TestCase("Data Source= /test/datasource/folder")]
	[TestCase("Data Source =/test/datasource/folder")]
	public void CanParse_WithOnlyDatasource(string connectionString)
	{
		AllaOptions options = AllaOptions.FromConnectionString(connectionString);

		Assert.That(options.Datasource, Is.EqualTo("/test/datasource/folder"));
	}

	[TestCase("DataSource = /test/datasource/folder")]
	[TestCase("data source = /test/datasource/folder")]
	[TestCase("datasource = /test/datasource/folder")]
	[TestCase("dataSource = /test/datasource/folder")]
	[TestCase("Datasource = /test/datasource/folder")]
	[TestCase("Data source = /test/datasource/folder")]
	public void Malformed_WithOnlyDatasource(string connectionString)
	{
		Assert.Throws<Exception>(() =>
		{
			AllaOptions.FromConnectionString(connectionString);
		});
	}

	[TestCase("Data Source = /test/datasource/folder, Pretty Print = true")]
	[TestCase("Data Source = /test/datasource/folder, Pretty Print= true")]
	[TestCase("Data Source = /test/datasource/folder, Pretty Print =true")]
	public void CanParse_WithPrettyPrint(string connectionString)
	{
		AllaOptions options = AllaOptions.FromConnectionString(connectionString);

		Assert.That(options.PrettyPrint, Is.True);
	}

	[TestCase("Data Source = /test/datasource/folder, Enum Strings = false")]
	[TestCase("Data Source = /test/datasource/folder, Enum Strings= false")]
	[TestCase("Data Source = /test/datasource/folder, Enum Strings =false")]
	public void CanParse_WithEnumStrings(string connectionString)
	{
		AllaOptions options = AllaOptions.FromConnectionString(connectionString);

		Assert.That(options.EnumStrings, Is.False);
	}

	[TestCase("Data Source = /test/datasource/folder, Enum Strings = false, Pretty Print = true")]
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