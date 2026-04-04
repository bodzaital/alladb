namespace AllaDb;

public interface IAllaSerializer
{
	void EnsureCreated(Alla db);

	List<Collection> Load(Alla db);

	void Persist(Alla db);
}