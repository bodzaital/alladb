namespace AllaDb.Tests;

public class CollectionClearTests : TestsBase
{
	[Test]
	public void CanClear_WithNoTransaction()
	{
		Collection collection = CreateTestCollection();
		collection.Add(CreateTestField());

		Assert.That(collection.GetDocuments(), Has.Count.EqualTo(1));

		collection.Clear();

		Assert.That(collection.GetDocuments(), Is.Empty);
	}

	[Test]
	public void CanClear_WithOneTransaction_WithExplicitCommit()
	{
		Collection collection = CreateTestCollection();
		collection.Add(CreateTestField());

		Assert.That(collection.GetDocuments(), Has.Count.EqualTo(1));

		Transaction transaction = collection.CreateTransaction();

		collection.Clear();

		using (Assert.EnterMultipleScope())
		{
			Assert.That(collection.HasTransactions, Is.True);
			Assert.That(collection.GetDocuments(), Is.Empty);
		}

		transaction.Commit().Dispose();

		using (Assert.EnterMultipleScope())
		{
			Assert.That(collection.HasTransactions, Is.False);
			Assert.That(collection.GetDocuments(), Is.Empty);
		}
	}

	[Test]
	public void CanClear_WithOneTransaction_WithExplicitRollback()
	{
		Collection collection = CreateTestCollection();
		collection.Add(CreateTestField());

		Assert.That(collection.GetDocuments(), Has.Count.EqualTo(1));

		Transaction transaction = collection.CreateTransaction();

		collection.Clear();

		using (Assert.EnterMultipleScope())
		{
			Assert.That(collection.HasTransactions, Is.True);
			Assert.That(collection.GetDocuments(), Is.Empty);
		}

		transaction.Rollback().Dispose();

		using (Assert.EnterMultipleScope())
		{
			Assert.That(collection.HasTransactions, Is.False);
			Assert.That(collection.GetDocuments(), Has.Count.EqualTo(1));
		}
	}
}