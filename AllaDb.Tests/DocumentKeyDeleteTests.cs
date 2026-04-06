namespace AllaDb.Tests;

public class DocumentKeyDeleteTests : DocumentTests
{
    [Test]
    public void CanDeleteKey_WithNoTransaction_WithTryGetValue()
    {
        Collection collection = CreateTestCollection();
        Document document = collection.Add(CreateTestField());

        document.Remove("key");
        bool resultExists = document.TryGetValue("key", out string? resultValue);

		using (Assert.EnterMultipleScope())
		{
			Assert.That(resultExists, Is.False);
			Assert.That(resultValue, Is.Null);
		}
    }

    [Test]
    public void CanDeleteKey_WithNoTransaction_WithGetValue()
    {
        Collection collection = CreateTestCollection();
        Document document = collection.Add(CreateTestField());

        document.Remove("key");
        string? resultValue = document.GetValue<string?>("key");

		using (Assert.EnterMultipleScope())
		{
			Assert.That(resultValue, Is.Null);
		}
    }

    [Test]
    public void CanDeleteKey_WithOneTransaction_WithExplicitRollback()
    {
        Collection collection = CreateTestCollection();
        Document document = collection.Add(CreateTestField());

        Transaction transaction = collection.CreateTransaction();
        document.Remove("key");
        bool resultExists = document.TryGetValue("key", out string? resultValue);

		using (Assert.EnterMultipleScope())
		{
			Assert.That(collection.HasTransactions, Is.True);
			Assert.That(resultExists, Is.False);
			Assert.That(resultValue, Is.Null);
		}

		transaction.Rollback().Dispose();

        resultExists = document.TryGetValue("key", out resultValue);

		using (Assert.EnterMultipleScope())
		{
			Assert.That(collection.HasTransactions, Is.False);
			Assert.That(resultExists, Is.True);
			Assert.That(resultValue, Is.EqualTo("original value"));
		}
	}

    [Test]
    public void CanDeleteKey_WithOneTransaction_WithExplicitCommit()
    {
        Collection collection = CreateTestCollection();
        Document document = collection.Add(CreateTestField());

        Transaction transaction = collection.CreateTransaction();
        document.Remove("key");
        bool resultExists = document.TryGetValue("key", out string? resultValue);

		using (Assert.EnterMultipleScope())
		{
			Assert.That(collection.HasTransactions, Is.True);
			Assert.That(resultExists, Is.False);
			Assert.That(resultValue, Is.Null);
		}

		transaction.Commit().Dispose();

        resultExists = document.TryGetValue("key", out resultValue);

		using (Assert.EnterMultipleScope())
		{
			Assert.That(collection.HasTransactions, Is.False);
			Assert.That(resultExists, Is.False);
			Assert.That(resultValue, Is.Null);
		}
	}
}
