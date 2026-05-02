namespace AllaDb.Tests;

public class TransactionTests : TestsBase
{
    [Test]
    public void CanHandleTransaction()
    {
        Collection collection = CreateTestCollection();

        using Transaction tx = collection.CreateTransaction();

        Assert.That(collection.HasTransactions, Is.True);

        tx.Dispose();

        Assert.That(collection.HasTransactions, Is.False);
    }

    [Test]
    public void CanHandleTwoTransactions()
    {
        Collection collection = CreateTestCollection();

        using Transaction tx1 = collection.CreateTransaction();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(collection.HasTransactions, Is.True);
            Assert.That(collection.Transactions, Has.Count.EqualTo(1));
        }

        using Transaction tx2 = collection.CreateTransaction();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(collection.HasTransactions, Is.True);
            Assert.That(collection.Transactions, Has.Count.EqualTo(2));
        }

        tx2.Dispose();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(collection.HasTransactions, Is.True);
            Assert.That(collection.Transactions, Has.Count.EqualTo(1));
        }

        tx1.Dispose();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(collection.HasTransactions, Is.False);
            Assert.That(collection.Transactions, Has.Count.EqualTo(0));
        }
    }

    [Test]
    public void CannotDisposeOutOfOrder()
    {
        Collection collection = CreateTestCollection();

        using Transaction tx1 = collection.CreateTransaction();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(collection.HasTransactions, Is.True);
            Assert.That(collection.Transactions, Has.Count.EqualTo(1));
        }

        using Transaction tx2 = collection.CreateTransaction();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(collection.HasTransactions, Is.True);
            Assert.That(collection.Transactions, Has.Count.EqualTo(2));
        }

        Assert.Throws<Exception>(tx1.Dispose);
    }
}