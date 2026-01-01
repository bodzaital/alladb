using System.Collections.ObjectModel;

namespace AllaDb;

public interface IAlla
{
	void DropDatabase();
	
	void DropCollection(string collectionName);

	Collection GetCollection(string collectionName);

	ReadOnlyCollection<Collection> GetCollections();

	void Persist();
}