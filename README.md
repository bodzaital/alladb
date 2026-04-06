# AllaDb

*A Limited, Lightweight Application DB*

A very simple document "database" written in C# and serializing to JSON by default.

## Quick Start

Create a new database using a connection string:

```c#
using AllaDb;

string connectionString = "Data Source = database.json";
Alla db = new(AllaOptions.FromConnectionString(connectionString));
```

Grab a reference to a collection:

```c#
Collection myCollection = db.GetCollection("my_collection");
```

Create documents in the collection:

```c#
Document firstDocument = myCollection.Add(new()
{
	{ "key1", "value1" },
	{ "key2", "value2" },
});
```

Document fields are backed by a dictionary of string key, nullable object value field. Retrieving data requires to know the type of the value:

```c#
string value = firstDocument.GetField<string>("key1");

bool hasField = firstDocument.TryGetField<string>("key2", out string? value);
```

Transactions are implemented with the disposable interface. The default transaction resolution is to commit. More than one transactions can be opened.

```c#
using (Transaction transaction1 = myCollection.CreateTransaction())
{
	firstDocument.Remove("key1");

	Transaction transaction2 = myCollection.CreateTransaction();
	
	myCollection.Remove(firstDocument);

	transaction2.Rollback().Dispose();

	transaction1.Commit();
}
```