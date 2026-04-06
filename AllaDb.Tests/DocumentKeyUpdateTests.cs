namespace AllaDb.Tests;

public class DocumentKeyUpdateTests : DocumentTests
{
    [Test]
    public void CanUpdateKey_WithNoTransaction_WithTryGetValue()
    {
        Collection collection = CreateTestCollection();
        Document document = collection.Add(CreateTestField());

        document.AddOrUpdate("key", "updated value");
        document.TryGetValue("key", out string? result);

		using (Assert.EnterMultipleScope())
		{
			Assert.That(collection.HasTransactions, Is.False);
			Assert.That(result, Is.EqualTo("updated value"));
		}
    }

    [Test]
    public void CanUpdateKey_WithNoTransaction_WithGetValue()
    {
        Collection collection = CreateTestCollection();
        Document document = collection.Add(CreateTestField());

        document.AddOrUpdate("key", "updated value");
        string? result = document.GetValue<string?>("key");

		using (Assert.EnterMultipleScope())
		{
			Assert.That(collection.HasTransactions, Is.False);
			Assert.That(result, Is.EqualTo("updated value"));
		}
    }

    [Test]
    public void CanUpdateKey_WithOneTransaction_WithExplicitRollback()
    {
        Collection collection = CreateTestCollection();
        Document document = collection.Add(CreateTestField());

        Transaction transaction = collection.CreateTransaction();
        document.AddOrUpdate("key", "updated value");
        document.TryGetValue("key", out string? result);

		using (Assert.EnterMultipleScope())
		{
			Assert.That(collection.HasTransactions, Is.True);
			Assert.That(result, Is.EqualTo("updated value"));
		}

		transaction.Rollback().Dispose();

        document.TryGetValue("key", out result);

		using (Assert.EnterMultipleScope())
		{
			Assert.That(collection.HasTransactions, Is.False);
			Assert.That(result, Is.EqualTo("original value"));
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
			Assert.That(collection.HasTransactions, Is.True);
			Assert.That(result, Is.EqualTo("updated value"));
		}

		transaction.Commit().Dispose();

        document.TryGetValue("key", out result);

		using (Assert.EnterMultipleScope())
		{
			Assert.That(collection.HasTransactions, Is.False);
			Assert.That(result, Is.EqualTo("updated value"));
		}
	}
}
