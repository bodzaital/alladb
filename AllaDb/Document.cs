using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace AllaDb;

/// <summary>Represents a set of fields.</summary>
public class Document(Dictionary<string, object?> fields)
{
	internal Collection? Collection { get; set; }

	/// <summary>Gets the unique identifier of this <see cref="Document"/>.</summary>
	[JsonInclude]
	public string Id { get; internal set; } = Guid.NewGuid().ToString();

	[JsonInclude]
	internal Dictionary<string, object?> Fields { get; set; } = fields;

	/// <summary>Determines whether the <see cref="Document"/> contains the specified field.</summary>
	/// <param name="key">The key of the field to locate in the <see cref="Document"/>.</param>
	/// <returns><see cref="true"/> if the <see cref="Document"/> contains a field with the specified key; otherwise, <see cref="false"/>.</returns>
	public bool HasField(string key) => GetFields().ContainsKey(key);

	/// <summary>Gets the value associated with the specified field.</summary>
	/// <typeparam name="T">Target type of the value associated with the specified field.</typeparam>
	/// <param name="key">The key of the field to get.</param>
	/// <returns>The value associated with the specified field. If the specified field is not found, throws an <see cref="ArgumentOutOfRangeException"/>.</returns>
	public T? GetField<T>(string key)
	{
		if (!HasField(key)) throw new ArgumentOutOfRangeException(nameof(key), key);
		return (T?)Convert.ChangeType(GetFields()[key], typeof(T));
	}

	/// <summary>Sets the value associated with the specified field.</summary>
	/// <param name="key">The key of the field to set.</param>
	/// <param name="value">Value of the field to set. The value can be <see cref="null"/>.</param>
	public void SetField(string key, object? value)
	{
		Collection!.ThrowIfRequiredTransactionMissing();
		Collection!.Constraints.ForEach((x) => x.ValidateFieldWrite(key, value));

		if (Collection!.OpenTransaction is null)
		{
			if (!Fields.TryAdd(key, value)) Fields[key] = value;
			return;
		}

		Collection.OpenTransaction.AddFieldWrite(Id, key, value);
	}

	/// <summary>Deletes the field with the specified key from the <see cref="Document"/>. If the specified field is not found, throws an <see cref="ArgumentOutOfRangeException"/>.</summary>
	/// <param name="key">The key of the field to delete.</param>
	public void DeleteField(string key)
	{
		Collection!.ThrowIfRequiredTransactionMissing();
		Collection!.Constraints.ForEach((x) => x.ValidateFieldDelete(key));
		
		if (Collection!.OpenTransaction is null)
		{
			bool couldRemove = Fields.Remove(key);
			if (!couldRemove) throw new ArgumentOutOfRangeException(nameof(key), key);
			return;
		}

		Collection.OpenTransaction.AddFieldDeletion(Id, key);
	}

	/// <summary>Gets the value associated with the specified field.</summary>
	/// <typeparam name="T">Target type of the value associated with the specified field.</typeparam>
	/// <param name="key">The key of the field to get.</param>
	/// <param name="value">When this method returns, contains the value associated with the specified field, if the key is found, otherwise, the default value for <typeparamref name="T"/>.</param>
	/// <returns><see cref="true"/> if the <see cref="Document"/> contains a field with the specified key, otherwise, <see cref="false"/>.</returns>
	public bool TryGetField<T>(string key, out T? value)
	{
		bool hasField = HasField(key);

		value = hasField
			? GetField<T>(key)
			: default;

		return hasField;
	}

	/// <summary>Returns a <see cref="ReadOnlyDictionary{string, object?}"/> of the fields of the <see cref="Document"/>.</summary>
	/// <returns>A <see cref="ReadOnlyDictionary{string, object?}"/> that can be used to iterate over the fields of the <see cref="Document"/>.</returns>
	public ReadOnlyDictionary<string, object?> GetFields()
	{
		if (Collection?.OpenTransaction is null) return new(Fields);

		List<Transaction.FieldChange> fieldDeletions = Collection.OpenTransaction.FieldChanges(Transaction.ChangeAction.Deleted);
		List<Transaction.FieldChange> fieldWrites = Collection.OpenTransaction.FieldChanges(Transaction.ChangeAction.Written);

		IEnumerable<KeyValuePair<string, object?>> unchangedFields = Fields
			.Where((field) => fieldDeletions.Any((deletion) => deletion.Key == field.Key))
			.Where((field) => fieldWrites.Any((write) => write.Key == field.Key));
		
		IEnumerable<KeyValuePair<string, object?>> writtenFields = fieldWrites
			.Select((write) => new KeyValuePair<string, object?>(write.Key, write.Value));
		
		IEnumerable<KeyValuePair<string, object?>> txFields = [..unchangedFields, ..writtenFields];
		
		return new(txFields.ToDictionary());
	}
}