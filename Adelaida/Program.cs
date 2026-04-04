using AllaDb;

string connectionString = "Data Source = database.json, Enum Strings = true, Pretty Print = true, Partition Options: Strategy = ByCollection, Partition Options: Max Size = 1";

Alla db = new(AllaOptions.FromConnectionString(connectionString));

// Collection coll1 = db.GetCollection("coll1");
// coll1.Add(new(){
// 	{ "key11", "val11" },
// });
// coll1.Add(new(){
// 	{ "key12", "val12" },
// });

// Collection coll2 = db.GetCollection("coll2");
// coll2.Add(new(){
// 	{ "key21", "val21" },
// });
// coll2.Add(new(){
// 	{ "key22", "val22" },
// });

db.Persist();