using System.Text.Json.Serialization;

namespace AllaDb;

public interface IConstraint
{
	void SetCollection(Collection collection);
	void ValidateNewDocument(Dictionary<string, object?> fields);
	public void ValidateFieldWrite(string key, object? value);
	void ValidateFieldDelete(string key);
}

public sealed class Constraint(string key, Constraint.Type type, params List<object?> constraintValues) : IConstraint
{
	public enum Type
	{
		Required,
		Unique,
		Default,
		From,
	}

	private Collection? _collection;
	
	[JsonInclude]
	internal string Key { get; set; } = key;
	
	[JsonInclude]
	internal Type ConstraintType { get; set; } = type;

	[JsonInclude]
	internal List<object?> Values { get; set; } = constraintValues;

	public void ValidateNewDocument(Dictionary<string, object?> fields)
	{
		if (ConstraintType == Type.Required)
		{
			if (!fields.ContainsKey(Key)) throw new Exception();
		}

		if (ConstraintType == Type.Unique)
		{
			bool exists = _collection!.Documents
				.Where((x) => x.HasField(Key))
				.Select((x) => x.GetField<object?>(Key))
				.Contains(fields[Key]);
			
			if (exists) throw new Exception();
		}

		if (ConstraintType == Type.Default)
		{
			if (!fields.TryAdd(Key, Values.First())) fields[Key] = Values.First();
		}

		if (ConstraintType == Type.From)
		{
			bool isFromList = Values.Contains(fields[Key]);

			if (!isFromList) throw new Exception();
		}
	}

	public void ValidateFieldWrite(string key, object? value)
	{
		if (key != Key) return;

		if (ConstraintType == Type.Unique)
		{
			bool exists = _collection!.Documents
				.Where((x) => x.HasField(Key))
				.Select((x) => x.GetField<object?>(Key))
				.Contains(value);
			
			if (exists) throw new Exception();
		}

		if (ConstraintType == Type.From)
		{
			bool isFromList = Values.Contains(value);

			if (!isFromList) throw new Exception();
		}
	}

	public void ValidateFieldDelete(string key)
	{
		if (key != Key) return;

		if (ConstraintType == Type.Required)
		{
			throw new Exception();
		}
	}

	public void SetCollection(Collection collection) => _collection = collection;
}