Rework:

Alla : IEnumerable
- constructor(AllaOptions options)
- DropDatabase()
- DropCollection(string name)
- Persist()
- GetEnumerator()
- GetCollection(string name)

Collection : IEnumerable
- Clear()
- Add(Document document)
- Remove(Document document)
- RemoveAll(Predicate<Document> predicate) ???
- CreateTransaction()
- GetEnumerator()

Transaction : IDisposable
- Dispose()
- MarkForCommit()
- MarkForDelete()

Document : IEnumerable
- ContainsKey(string key)
- Remove(string key)
- TryGetValue<T>(string key, out T? value)
- AddOrUpdate(string key, object? value)
- GetEnumerator()

AllaOptions
- constructor(string datasource, bool prettyPrint, bool enumStrings, PartitionOptions partitionOptions)
- static FromConnectionString(string connectionString)

PartitionOptions(PartitionBy Strategy, int Size, int? BatchSize)

enum PartitionBy
- Documents
- Partitions