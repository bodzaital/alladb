using System.Text.Json;
using AllaDb;

Alla db = new(new("database.json")
{
	EnumStrings = true,
	PrettyPrint = true,
	PartitionOptions = new()
	{
		Strategy = PartitionStrategy.ByCollection,
		MaxSize = 1,
	},
});

Collection coll1 = db.GetCollection("coll1");
coll1.Add(new(){
	{ "key11", "val11" },
});
coll1.Add(new(){
	{ "key12", "val12" },
});

Collection coll2 = db.GetCollection("coll2");
coll2.Add(new(){
	{ "key21", "val21" },
});
coll2.Add(new(){
	{ "key22", "val22" },
});

db.Persist();