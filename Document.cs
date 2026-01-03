using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace AllaDb;

public class Document(Dictionary<string, object?> fields)
{
	internal Collection? Collection { get; set; }

	[JsonInclude]
	public string Id { get; internal set; } = Guid.NewGuid().ToString();

	[JsonInclude]
	internal Dictionary<string, object?> Fields { get; set; } = fields;

	public bool HasField(string key) => GetFields().ContainsKey(key);

	public T? GetField<T>(string key)
	{
		if (!HasField(key)) throw new ArgumentOutOfRangeException(nameof(key), key);
		return (T?)Convert.ChangeType(GetFields()[key], typeof(T));
	}

	public void SetField<T>(string key, T value)
	{
		Collection!.ThrowIfRequiredTransactionMissing();

		if (Collection!.Transaction is null)
		{
			if (!Fields.TryAdd(key, value)) Fields[key] = value;
			return;
		}

		Collection.Transaction.AddFieldWrite(Id, key, value);
	}

	public void DeleteField(string key)
	{
		Collection!.ThrowIfRequiredTransactionMissing();
		
		if (Collection!.Transaction is null)
		{
			bool couldRemove = Fields.Remove(key);
			if (!couldRemove) throw new ArgumentOutOfRangeException(nameof(key), key);
			return;
		}

		Collection.Transaction.AddFieldDeletion(Id, key);
	}

	public bool TryGetField<T>(string key, out T? value)
	{
		bool hasField = HasField(key);

		value = hasField
			? GetField<T>(key)
			: default;

		return hasField;
	}

	private ReadOnlyDictionary<string, object?> GetFields()
	{
		if (Collection?.Transaction is null) return new(Fields);

		List<Transaction.FieldChange> fieldDeletions = Collection.Transaction.FieldChanges(Transaction.ChangeAction.Deleted);
		List<Transaction.FieldChange> fieldWrites = Collection.Transaction.FieldChanges(Transaction.ChangeAction.Written);

		IEnumerable<KeyValuePair<string, object?>> unchangedFields = Fields
			.Where((field) => fieldDeletions.Any((deletion) => deletion.Key == field.Key))
			.Where((field) => fieldWrites.Any((write) => write.Key == field.Key));
		
		IEnumerable<KeyValuePair<string, object?>> writtenFields = fieldWrites
			.Select((write) => new KeyValuePair<string, object?>(write.Key, write.Value));
		
		IEnumerable<KeyValuePair<string, object?>> txFields = [..unchangedFields, ..writtenFields];
		
		return new(txFields.ToDictionary());
	}
}