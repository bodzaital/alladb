namespace AllaDb.Tests;

public abstract class CollectionTests
{
    protected static Collection CreateTestCollection() => new()
    {
        Name = "Test collection"
    };

    protected static Dictionary<string, object?> CreateTestField() => new()
    {
        { "key", "original value" },
    };
}