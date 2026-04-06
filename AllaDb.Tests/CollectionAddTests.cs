namespace AllaDb.Tests;

public class CollectionAddTests : TestsBase
{
	[Test]
	public void CanAdd_WithNoTransaction()
	{
		Collection collection = CreateTestCollection();

		Assert.That(collection.Count, Is.EqualTo(0));

		collection.Add(CreateTestField());

		Assert.That(collection.Count, Is.EqualTo(1));
	}

	[Test]
	public void CanAdd_WithOneTransaction_WithExplicitCommit()
	{
		Collection collection = CreateTestCollection();

		Assert.That(collection.Count, Is.EqualTo(0));
		Assert.That(collection.HasTransactions, Is.False);

		Transaction transaction = collection.CreateTransaction();

		collection.Add(CreateTestField());

		Assert.That(collection.Count, Is.EqualTo(1));
		Assert.That(collection.HasTransactions, Is.True);

		transaction.Commit().Dispose();

		Assert.That(collection.Count, Is.EqualTo(1));
		Assert.That(collection.HasTransactions, Is.False);
	}

	[Test]
	public void CanAdd_WithOneTransaction_WithExplicitRollback()
	{
		Collection collection = CreateTestCollection();

		Assert.That(collection.Count, Is.EqualTo(0));
		Assert.That(collection.HasTransactions, Is.False);

		Transaction transaction = collection.CreateTransaction();

		collection.Add(CreateTestField());

		Assert.That(collection.Count, Is.EqualTo(1));
		Assert.That(collection.HasTransactions, Is.True);

		transaction.Rollback().Dispose();

		Assert.That(collection.Count, Is.EqualTo(0));
		Assert.That(collection.HasTransactions, Is.False);
	}
}