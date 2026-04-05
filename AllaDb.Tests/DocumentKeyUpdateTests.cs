namespace AllaDb.Tests;

public class DocumentKeyUpdateTests
{
    [Test]
    public void CanUpdateKey_WithNoTransaction()
    {
        Collection collection = CreateTestCollection();
        Document document = collection.Add(CreateTestField());

        document.AddOrUpdate("key", "updated value");
        document.TryGetValue("key", out string? result);

		using (Assert.EnterMultipleScope())
		{
			Assert.That(collection.HasTransaction, Is.False);
			Assert.That(result, Is.EqualTo("updated value"));
		}
    }

    [Test]
    public void CanUpdateKey_WithOneTransaction_WithRollback()
    {
        Collection collection = CreateTestCollection();
        Document document = collection.Add(CreateTestField());

        Transaction transaction = collection.CreateTransaction();
        document.AddOrUpdate("key", "updated value");
        document.TryGetValue("key", out string? result);

		using (Assert.EnterMultipleScope())
		{
			Assert.That(collection.HasTransaction, Is.True);
			Assert.That(result, Is.EqualTo("updated value"));
		}

		transaction.Rollback().Dispose();

        document.TryGetValue("key", out result);

		using (Assert.EnterMultipleScope())
		{
			Assert.That(collection.HasTransaction, Is.False);
			Assert.That(result, Is.EqualTo("original value"));
		}
	}

    [Test]
    public void CanUpdateKey_WithOneTransaction_WithDefaultCommit()
    {
        Collection collection = CreateTestCollection();
        Document document = collection.Add(CreateTestField());

        Transaction transaction = collection.CreateTransaction();
        document.AddOrUpdate("key", "updated value");
        document.TryGetValue("key", out string? result);

		using (Assert.EnterMultipleScope())
		{
			Assert.That(collection.HasTransaction, Is.True);
			Assert.That(result, Is.EqualTo("updated value"));
		}

		transaction.Dispose();

        document.TryGetValue("key", out result);

		using (Assert.EnterMultipleScope())
		{
			Assert.That(collection.HasTransaction, Is.False);
			Assert.That(result, Is.EqualTo("updated value"));
		}
	}

    [Test]
    public void CanUpdateKey_WithOneTransaction_WithExplicitCommit()
    {
        Collection collection = CreateTestCollection();
        Document document = collection.Add(CreateTestField());

        Transaction transaction = collection.CreateTransaction();
        document.AddOrUpdate("key", "updated value");
        document.TryGetValue("key", out string? result);

		using (Assert.EnterMultipleScope())
		{
			Assert.That(collection.HasTransaction, Is.True);
			Assert.That(result, Is.EqualTo("updated value"));
		}

		transaction.Commit().Dispose();

        document.TryGetValue("key", out result);

		using (Assert.EnterMultipleScope())
		{
			Assert.That(collection.HasTransaction, Is.False);
			Assert.That(result, Is.EqualTo("updated value"));
		}
	}

    [Test]
    public void CanUpdateKey_WithTwoTransactions_WithRollback()
    {
        Collection collection = CreateTestCollection();
        Document document = collection.Add(CreateTestField());

        Transaction transaction1 = collection.CreateTransaction();

        document.AddOrUpdate("key", "first update");
        document.TryGetValue("key", out string? result);

		using (Assert.EnterMultipleScope())
		{
			Assert.That(collection.HasTransaction, Is.True);
			Assert.That(result, Is.EqualTo("first update"));
		}

		Transaction transaction2 = collection.CreateTransaction();

        document.AddOrUpdate("key", "second update");
        document.TryGetValue("key", out result);

		using (Assert.EnterMultipleScope())
		{
			Assert.That(collection.HasTransaction, Is.True);
			Assert.That(result, Is.EqualTo("second update"));
		}

		transaction2.Rollback().Dispose();

        document.TryGetValue("key", out result);

		using (Assert.EnterMultipleScope())
		{
			Assert.That(collection.HasTransaction, Is.True);
			Assert.That(result, Is.EqualTo("first update"));
		}

		transaction1.Rollback().Dispose();

        document.TryGetValue("key", out result);

		using (Assert.EnterMultipleScope())
		{
			Assert.That(collection.HasTransaction, Is.False);
			Assert.That(result, Is.EqualTo("original value"));
		}
	}

    private static Collection CreateTestCollection() => new()
    {
        Name = "Test collection"
    };

    private static Dictionary<string, object?> CreateTestField() => new()
    {
        { "key", "original value" },
    };
}
