# AllaDb

Something that can barely be called a document "database," written in C# and using JSON.

Create a new DB using the constructor:

```c#
Alla db = new("file.json");
```

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