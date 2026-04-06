using NSubstitute;

namespace AllaDb.Tests;

public class AllaTests
{
	private readonly IAllaSerializer _testSerializer = Substitute.For<IAllaSerializer>();
	private Alla? _db;

	[SetUp]
	public void SetUp()
	{
	}

	[Test]
	public void CanDropCollection()
	{
		CreateSimpleDatabase();

		Assert.That(_db!.Collections.Count((x) => x.Name == "collection1"), Is.EqualTo(1));

		_db!.DropCollection("collection1");

		Assert.That(_db!.Collections.Count((x) => x.Name == "collection1"), Is.EqualTo(0));
	}

	[Test]
	public void CanCreateCollectionIfDoesNotExist()
	{
		CreateEmptyDatabase();

		Assert.That(_db!.Collections, Is.Empty);

		Collection collection = _db!.GetCollection("collection");

		Assert.That(_db!.Collections, Has.Count.EqualTo(1));
	}

	[Test]
	public void CanGetCollectionIfExists()
	{
		CreateSimpleDatabase();

		Assert.That(_db!.Collections.Count((x) => x.Name == "collection1"), Is.EqualTo(1));

		Collection collection = _db!.GetCollection("collection1");

		Assert.That(_db!.Collections.Count((x) => x.Name == "collection1"), Is.EqualTo(1));
	}

	[Test]
	public void DoesNotPersistInMemoryDatabase()
	{
		CreateInMemoryDatabase();

		Assert.Throws<Exception>(() => _db!.Persist());
	}

	[Test]
	public void DoesNotPersistWhenHasOpenTransaction()
	{
		CreateSimpleDatabase();

		_db!.Collections.First().CreateTransaction();

		Assert.Throws<Exception>(() => _db!.Persist());
	}

	[Test]
	public void CanPersist()
	{
		CreateSimpleDatabase();

		_db!.Persist();
	}

	private void CreateEmptyDatabase()
	{
		_testSerializer.Load(Arg.Any<Alla>()).Returns([]);
		_db = new(AllaOptions.FromConnectionString("Data Source = database.json"), _testSerializer);
	}

	private void CreateInMemoryDatabase()
	{
		_testSerializer.Load(Arg.Any<Alla>()).Returns([]);
		_db = new(AllaOptions.FromConnectionString("Data Source = :memory:"), _testSerializer);
	}

	private void CreateSimpleDatabase()
	{
		List<Collection> collections = [
			new Collection() { Name = "collection1" },
			new Collection() { Name = "collection2" },
		];

		// Three documents in collection1.
		collections.First().Add(new()
		{
			{ "key111", "val111" },
			{ "key112", "val112" },
		});

		collections.First().Add(new()
		{
			{ "key121", "val121" },
			{ "key122", "val122" },
		});

		collections.First().Add(new()
		{
			{ "key131", "val131" },
			{ "key132", "val132" },
		});

		// Two documents in collection2.
		collections.First().Add(new()
		{
			{ "key211", "val211" },
			{ "key212", "val212" },
		});

		collections.First().Add(new()
		{
			{ "key221", "val221" },
			{ "key222", "val222" },
		});

		_testSerializer.Load(Arg.Any<Alla>()).Returns(collections);
		_db = new(AllaOptions.FromConnectionString("Data Source = database.json"), _testSerializer);
	}
}