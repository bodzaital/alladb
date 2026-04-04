namespace AllaDb;

/// <summary>A serializer interface that, when implemented, can be used to customize the serializer of the database.</summary>
public interface IAllaSerializer
{
	/// <summary>Called when the database is initialized, ensures that at least empty database files exist.</summary>
	void EnsureCreated(Alla db);

	/// <summary>Loads the database files and returned the deserialized collections.</summary>
	List<Collection> Load(Alla db);

	/// <summary>Saves the database to files.</summary>
	void Persist(Alla db);
}