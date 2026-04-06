namespace AllaDb.Tests;

public class CollectionRemoveAllTests : TestsBase
{
	[Test]
	public void CanRemoveAll_WithNoTransaction()
	{
		Collection collection = CreateTestCollection();
		collection.Add(CreateTestField());
		collection.Add(CreateTestField());

		Assert.That(collection.Count, Is.EqualTo(2));

		collection.Clear();

		Assert.That(collection.Count, Is.EqualTo(0));
	}

	[Test]
	public void CanRemoveAll_WithOneTransaction_WithExplicitCommit()
	{
		Collection collection = CreateTestCollection();
		collection.Add(CreateTestField());
		collection.Add(CreateTestField());

		Transaction transaction = collection.CreateTransaction();

		using (Assert.EnterMultipleScope())
		{
			Assert.That(collection.HasTransactions, Is.True);
			Assert.That(collection.Count, Is.EqualTo(2));
		}

		collection.Clear();

		using (Assert.EnterMultipleScope())
		{
			Assert.That(collection.HasTransactions, Is.True);
			Assert.That(collection.Count, Is.EqualTo(0));
		}

		transaction.Commit().Dispose();

		using (Assert.EnterMultipleScope())
		{
			Assert.That(collection.HasTransactions, Is.False);
			Assert.That(collection.Count, Is.EqualTo(0));
		}
	}

	[Test]
	public void CanRemoveAll_WithOneTransaction_WithExplicitRollback()
	{
		Collection collection = CreateTestCollection();
		collection.Add(CreateTestField());
		collection.Add(CreateTestField());

		Transaction transaction = collection.CreateTransaction();

		using (Assert.EnterMultipleScope())
		{
			Assert.That(collection.HasTransactions, Is.True);
			Assert.That(collection.Count, Is.EqualTo(2));
		}

		collection.Clear();

		using (Assert.EnterMultipleScope())
		{
			Assert.That(collection.HasTransactions, Is.True);
			Assert.That(collection.Count, Is.EqualTo(0));
		}

		transaction.Rollback().Dispose();

		using (Assert.EnterMultipleScope())
		{
			Assert.That(collection.HasTransactions, Is.False);
			Assert.That(collection.Count, Is.EqualTo(2));
		}
	}
}