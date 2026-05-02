# AllaDb

*A Limited, Lightweight Application DB*

A very simple document "database" written in C# and serializing to JSON by default.

## Quick Start

Create a new database using a connection string. The database is serialized based on the serializer, by default it is saved to a "database.json" file in the data source folder.

```c#
using AllaDb;

string connectionString = "Data Source = /example/datasource/folder";
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

## Adelaida CLI

The Adelaida project is a small CLI interface to database files. When called without arguments, the default connection string is `Data Source = .` meaning it will open the database.json file in the working directory.

### Arguments

- `-c` or `--connection`: Connection string for the database.
- `-v` or `--verbose`: Enable verbose logging that is written once the REPL normally exits.

Verbose logging is displayed when the REPL normally exits -- meaning that the `exit` function is called. There are not much actual logging yet.

### Usage

When starting the REPL, the following handle is displayed:

```
(no collection) > _
```

Typing `help` and hitting [Enter] will display a list of available functions. Description of functions can be invoked with `help [name of function]`, for example:

```
(no collection) > help get-collection
get-collection: Creates a collection in the database if the name does not already exist, or gets the collection in the database if the name already exists.
```

Function arguments are separated with a space, and multiple can be passed to certain functions:

```
(no collection) > add key1=value1 "key2=value with spaces"
```

The handle will appear differently based on the memory of the REPL. The REPL can load one collection, one document, and one transaction at the same time. The handle may have one or all of these decorations. The state of the REPL can be displayed using `status`.

If a collection is loaded (with `get-collection collection_name`):

```
(collection_name) > _
```

If the database is changed, but not yet saved (after `add key=value`)

```
(*collection_name) > _
```

If a document is loaded (with `get-document document_id`):

```
(collection_name editing document_id) > _
```

If a transaction is created (with `create-transaction`):

```
[collection_name] > _
```

### Functions

**Function over the database**

| Function name | Description | Arguments | Requires |
| ------------- | ----------- | --------- | -------- |
| `drop-database` | Removes all collections from the database. | ❌ | confirmation (y/n) |
| `drop-collection` | Removes the collection whose name matches the specified name. | name: name of the collection | arguments, confirmation (y/n) |
| `get-collections` | Get all collections in the database. | ❌ | ❌ |
| `get-collection` | Creates a collection in the database if the name does not already exist, or gets the collection in the database if the name already exists. | name: name of the collection | arguments |
| `persist` | Serializes the database based on the connection string and the serializer. | ❌ | no transaction |
| `status` | Shows simple information regarding the current session. | ❌ | ❌ |

**Functions over a collection**

| Function name | Description | Arguments | Requires |
| ------------- | ----------- | --------- | -------- |
| `clear` | Removes all documents from the collection. | ❌ | collection, confirmation (y/n) |
| `add` | Adds a new document with the specified fields to the end of the collection. | fields: list of key=value, optionally enclosed in " and the value typed with (T) prefix for primitive types | arguments, collection |
| `remove` | Removes the specific document from the collection. | ❌ | collection, document, confirmation (y/n) |
| `get-documents` | Get all documents of the collection. | ❌ | collection |
| `get-document` | Get the document associated with the specified ID. | id: ID of the document to get | arguments, collection |
| `close-collection` | Releaes the current collection from memory. | ❌ | collecion, no transaction |
| `create-transaction` | Creates a transaction over this collection | ❌ | collection, no transaction |
| `commit` | Resolves the transaction by commiting the changes. | ❌ | transaction |
| `roll-back` | Resolves the transaction by rolling back the changes. | ❌ | transaction |

**Functions over a document**

| Function name | Description | Arguments | Requires |
| ------------- | ----------- | --------- | -------- |
| `get-fields` | Get all fields of the document. | ❌ | collection, document |
| `remove-fields` | Removes the value with the specified key from the fields. | key: key of the field to remove | collection, document, arguments, confirmation (y/n) |
| `set-fields` | Adds a field to the document if the key does not already exist, or updates a field in the document if the key already exists. | fields: list of key=value, optionally enclosed in \" and the value typed with (T) prefix for primitive types | collection, document, arguments |
| `close-document` | Releases the current document from memory. | ❌ | collection, document |

## Reference

### AllaOptions

Namespace: `AllaDb`

Configuration for the database.

**Properties**

```c#
public string Datasource { get; set; }
```

Path to the database file. The special :memory: value will keep data in-memory and throw exceptions when attempting to persist.

```c#
public bool PrettyPrint { get; set; } = false;
```

If true, saves the database using line breaks and indents.

```c#
public bool EnumStrings { get; set; } = true;
```

If true, saves Enum values with constants instead of ordinals.

**Methods**

```c#
public static AllaOptions FromConnectionString(string connectionString)
```

Creates an instance of AllaOptions from a connection string.

### Alla

Namespace: `AllaDb`

A lightweight, limited application database storing key-value pairs.

**Methods**

```c#
public Alla(AllaOptions options, IAllaSerializer? serializer = null)
```

Creates a new instance of Alla Db.

Parameters:
- `AllaOptions options`: The instance of options to configure the database.
- `IAllaSerializer? serializer`: The serializer to use when persisting the database. If null, uses the default single-file serializer.

```c#
public IEnumerator GetEnumerator()
```

Exposes the enumerator of the underlying list of collections.

Returns: An enumerator for the list of collections.

```c#
public void DropDatabase()
```

Removes all collections from the database.

```c#
public void DropCollection(string name)
```

Removes the collection whose name matches the specified name.

Parameter:
- `string name`: The name of the collection to remove.

```c#
public Collection GetCollection(string name)
```

Creates a collection in the database if the name does not already exist, or gets the collection in the database if the name already exists.

Parameter:
- `string name`: The name of the collection to be created or whose collection should be retrieved.

Returns: The collection associated with the specified name.

```c#
public List<Collection> GetCollections()
```

Get all collections in the database.

Returns: All collections in the database.

```c#
public void Persist()
```

Serializes the database based on the connection string and the serializer. Throws an exception for in-memory databases.

### IAllaSerializer

Namespace: `AllaDb`

A serializer interface that, when implemented, can be used to customize the serializer of the database.

**Methods**

```c#
void EnsureCreated(Alla db)
```

Called when the database is initialized, ensures that at least an empty database file exists.

```c#
List<Collection> Load(Alla db)
```

Loads the database files and returns the deserialized collections.

```c#
void Persist(Alla db)
```

Saves the database to files.

### DefaultSerializer

Namespace: `AllaDb`

Implements `IAllaSerializer`

The default, single-file serializer used by the database when persisting. The database is serialized into a "database.json" file in the data source directory.

### CollectionSerializer

Namespace: `AllaDb`

Implements `IAllaSerializer`

A serializer that partitions by collections. The database is serialized into "database.[collection].json" files in the data source directory.

### Document

Namespace: `AllaDb`

**Properties**

```c#
public string Id { get; set; } = Guid.NewGuid().ToString()
```

Unique identifier of this document.

**Methods**

```c#
public IEnumerator GetEnumerator()
```

Exposes the enumerator of the underlying dictionary of key/value pairs.

```c#
public bool ContainsKey(string key)
```

Determines whether the document contains the specified key.

Parameter:
- `string key`: The key to locate in the fields.

Returns: true if the fields contains a value with the specified key; otherwise, false.

```c#
public void Remove(string key)
```

Removes the value with the specified key from the fields.

Parameter:
- `string key`: The key of the field to remove.

```c#
public T? GetValue<T>(string key)
```

Gets the value associated with the specified key.

Parameter:
- `string key`: The key of the field to remove.

Returns: The value associated with the specified key, if the key is found; otherwise, the default value for the type of the return value.

```c#
public bool TryGetValue<T>(string key, out T? value)
```

Gets the value associated with the specified key.

Parameters:
- `string key`: The key of the value to get.
- `out T? value`: When this method returns, contains the value associated with the specified key, if the key is found; otherwise, the default vale for the type of the value parameter. This parameter is passed uninitialized.

Returns: true if the fields contain a value with the specified key; otherwise, false.

```c#
public void AddOrUpdate(string key, object? value)
```

Adds a field to the document if the key does not already exist, or updates a field in the document if the key already exists.

Parameters:
- `string key`: The key to be added or whose value should be updated.
- `object? value`: The value to be added for an absent key or a new value for an existing key.

```c#
public Dictionary<string, object?> GetFields()
```

Get all fields of the document.

Returns: All fields of the document.

### Collection

Namespace: `AllaDb`

A collection of documents.

**Properties**

```c#
public required string Name { get; set; }
```

Unique name of this collection.

```c#
public bool HasTransactions { get; }
```

Returns true if this collection has any unresolved transactions.

**Methods**

```c#
public IEnumerator GetEnumerator()
```

Exposes the enumerator of the underlying list of documents.

```c#
public void Clear()
```

Removes all documents from the collection.

```c#
public Document Add(Dictionary<string, object?> fields)
```

Adds a new document with the specified fields to the end of the collection.

Parameter:
- `Dictionary<string, object?> fields`: The fields of a new document to be added to the end of the collection.

Returns: The new document in the collection.

```c#
public List<Document> AddRange(IEnumerable<Dictionary<string, object?>> fields)
```

Adds the specified list of fields to the end of the collection.

Parameter:
- `IEnumerable<Dictionary<string, object?>> fields`: The list of fields that should be added to the end of the collection.

Returns: The list of new documents in the collection.

```c#
public void Remove(Document document)
```

Removes the specific document from the collection.

Parameter:
- `Document document`: The document to remove from the collection.

```c#
public void RemoveAll(Func<Document, bool> predicate)
```

Removes all document that match the condition defined by the specified delegate.

Parameter:
- `Func<Document, bool> predicate`: The function delegate that defines the condition of the documents to remove.

```c#
public Document? GetDocument(string id)
```

Gets the document associated with the specified ID.

Parameter:
- `string id`: The ID of the document to get.

Returns: The document if the collection contains it by ID; otherwise, null.

```c#
public bool TryGetDocument(string id, [NotNullWhen(true)] out Document? document)
```

Gets the document associated with the specified ID.

Parameters:
- `string id`: The ID of the document to get.
- `[NotNullWhen(true)] out Document? document`: When this method returns, contains the document associated with the specified ID, if the ID is found; otherwise, null. This parameter is passed uninitialized.

Returns: true if the collection contains a document with the specified ID; otherwise, false.

```c#
public Transaction CreateTransaction()
```

Adds a new transaction over this collection to the end of the transaction stack.

Returns: The new transaction over this collection.

```c#
public List<Document> GetDocuments()
```

Get all documents of the collection.

Returns: All documents of the collection.

### Transaction

Namespace: `AllaDb`

Implements: `IDisposable`

Collection and document level transactions.

**Methods**

```c#
public void Dispose()
```

Resolves the transaction. If unmarked, the default resolution is to commit.

```c#
public Transaction Commit()
```

Marks this transaction to be committed. This is the default resolution.

```c#
public Transaction Rollback()
```

Marks this transaction to be rolled back.
