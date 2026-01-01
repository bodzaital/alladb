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

	public bool HasField(string key) => TxFields().ContainsKey(key);

	public T? GetField<T>(string key)
	{
		if (!HasField(key)) throw new ArgumentOutOfRangeException(nameof(key), key);
		return (T?)Convert.ChangeType(TxFields()[key], typeof(T));
	}

	public void SetField<T>(string key, T value)
	{
		if (Collection!.Transaction is null)
		{
			if (!Fields.TryAdd(key, value)) Fields[key] = value;
			return;
		}

		Transaction.FieldChange fieldChange = new(Id, key, value, Transaction.FieldChangeAction.Written);
		Collection.Transaction.FieldChanges.Add(fieldChange);
	}

	public void DeleteField(string key)
	{
		if (Collection!.Transaction is null)
		{
			bool couldRemove = Fields.Remove(key);
			if (!couldRemove) throw new ArgumentOutOfRangeException(nameof(key), key);
			return;
		}

		Transaction.FieldChange fieldChange = new(Id, key, null, Transaction.FieldChangeAction.Deleted);
		Collection.Transaction.FieldChanges.Add(fieldChange);
	}

	public bool TryGetField<T>(string key, out T? value)
	{
		bool hasField = HasField(key);

		value = hasField
			? GetField<T>(key)
			: default;

		return hasField;
	}

	public ReadOnlyDictionary<string, object?> GetFields() => TxFields();

	private ReadOnlyDictionary<string, object?> TxFields()
	{
		if (Collection?.Transaction is null) return new(Fields);

		IEnumerable<Transaction.FieldChange> fieldChanges = Collection.Transaction.FieldChanges
			.Where((x) => x.DocumentId == Id);

		IEnumerable<Transaction.FieldChange> fieldDeletions = fieldChanges
			.Where((x) => x.Action == Transaction.FieldChangeAction.Deleted);

		IEnumerable<Transaction.FieldChange> fieldWrites = fieldChanges
			.Where((x) => x.Action == Transaction.FieldChangeAction.Written);

		IEnumerable<KeyValuePair<string, object?>> unchangedFields = Fields
			.Where((field) => fieldDeletions.Any((deletion) => deletion.Key == field.Key))
			.Where((field) => fieldWrites.Any((write) => write.Key == field.Key));
		
		IEnumerable<KeyValuePair<string, object?>> writtenFields = fieldWrites
			.Select((write) => new KeyValuePair<string, object?>(write.Key, write.Value));
		
		IEnumerable<KeyValuePair<string, object?>> txFields = [..unchangedFields, ..writtenFields];
		
		return new(txFields.ToDictionary());
	}
}