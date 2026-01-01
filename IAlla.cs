namespace AllaDb;

public interface IAlla
{
	void DropDatabase();
	
	void DropCollection(string collectionName);

	Collection GetCollection(string collectionName);

	void Persist();
}