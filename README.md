# AllaDb

Something that can barely be called a document "database," written in C# and using JSON.

Create a new DB using the options:

```c#
AllaOptions options = new AllaOptions().AddDataSource("fields.json");
Alla db = new(options);
```

Or register and use it in dependency injection:

```c#
builder.Services.AddAllaDb((options) => options.AddDataSource("fields.json"));

// Consumer:
public class MyConsumerClass(IAlla db)
{
	// ...
}
```

Available options:
- AddDataSource(string dataSource): specify a relative path as the data source for persistance (otherwise it will be just an in-memory DB)
- WithRequiredTransactions(): specify that all collection/document level modifying operations require a transaction (otherwise transactions are not required)
- WithPrettyPrint(): specify that the serialized file is formatted (otherwise the file will not contain white space)
- WithEnumStrings(): specify that the serialization should use strings for enums (otherwise ordinals will be used)

Grab a reference to a collection (creates one if it does not exist):

```c#
Collection collection = db.GetCollection("collection name");
```

And then create documents in the collection:

```c#
Document doc1 = collection.CreateDocument(new()
{
	{ "key", "value" },
});
```

Document fields are backed by a `Dictionary<string, object?>` field, so anything goes. But you have to know the type:

```c#
string value = doc1.GetField<string>("key");
```

Use transactions to have the ability to roll back changes once control leaves the using block:

```c#
using (Transaction tx = collection.CreateTransaction())
{
	doc1.DeleteKey("key");
	collection.DeleteDocument(doc1.Id);

	// One of these must be called before the Transaction is disposed.
	// Transaction is resolved by the last resolution,
	// there are no checkpoints.
	tx.MarkForCommit();
	tx.MarkForRollback();
}
```

You can also use transactions without using blocks by calling Dispose manually:

```c#
Transaction tx = collection.CreateTransaction();

// ...

tx.MarkForCommit();
// or tx.MarkForRollback();

tx.Dispose();
```

Create constraints on a collection: 

```c#
collection.CreateConstraints(
	new Constraint("key", Constraint.Type.Required)
);
```

Available constraints:
- Required: key must exist (on create and delete)
- Unique: field must be unique among the collection (on create and update)
- Default: field has a default value (on create)
- From: field must have a value from a set (on create and update)

Create custom constraints by implementing IConstraint and throwing an exception when constraints are violated:

- `void SetCollection(Collection collection)` is called when deserializing the database and contains a reference to the associated collection
- `void ValidateNewDocument(Dictionary<string, object?> fields)` is called when a document is created
- `public void ValidateFieldWrite(string key, object? value)` is called when a field is written to
- `void ValidateFieldDelete(string key)` is called when a field is deleted

Mark fields and non-public properties with `[JsonInclude]`.