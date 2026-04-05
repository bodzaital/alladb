namespace AllaDb.Tests;

public abstract class DocumentTests
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