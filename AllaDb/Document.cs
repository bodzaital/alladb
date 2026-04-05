using System.Collections;
using System.Text.Json.Serialization;

namespace AllaDb;

/// <summary>A collection of key-value pairs identified by a unique GUID.</summary>
public class Document
{
	/// <summary>Unique identifier of this document.</summary>
	public string Id { get; set; } = Guid.NewGuid().ToString();

	internal Collection? Collection { get; set; }

	[JsonInclude]
	internal Dictionary<string, object?> Fields { get; set; } = [];

	/// <summary>Exposes the enumerator of the underlying dictionary of key-value pairs.</summary>
	public IEnumerator GetEnumerator() => GetFields().GetEnumerator();

	/// <summary>Determines whether the document contains the specified key.</summary>
	public bool ContainsKey(string key) => GetFields().ContainsKey(key);

	/// <summary>Removes the value with the specified key from the document.</summary>
	public void Remove(string key)
	{
		if (!Collection!.HasTransaction)
		{
			Fields.Remove(key);
			return;
		}

		Collection.Transactions.Last().Changes.Add(new(
			Transaction.Action.Delete,
			FieldChange: new(Id, key, null)
		));
	}

	/// <summary>Gets the value associated with the specified key.</summary>
	public T? GetValue<T>(string key)
	{
		bool doesContainKey = ContainsKey(key);

		return doesContainKey
			? (T?)Convert.ChangeType(GetFields()[key], typeof(T))
			: default;
	}

	/// <summary>Gets the value associated with the specified key.</summary>
	public bool TryGetValue<T>(string key, out T? value)
	{
		bool doesContainKey = ContainsKey(key);

		value = doesContainKey
			? (T?)Convert.ChangeType(GetFields()[key], typeof(T))
			: default;

		return doesContainKey;
	}

	/// <summary>Adds a key-value pair to the document if the key does not already exist, or updates a key-value pair in the document if the key already exists.</summary>
	public void AddOrUpdate(string key, object? value)
	{
		if (!Collection!.HasTransaction)
		{
			Fields[key] = value;
			return;
		}

		Collection.Transactions.Last().Changes.Add(new(
			Transaction.Action.Write,
			FieldChange: new(Id, key, value)
		));
	}

	private Dictionary<string, object?> GetFields()
	{
		if (!Collection!.HasTransaction) return Fields;

		return Collection.Transactions.Aggregate(new Dictionary<string, object?>(Fields), ReduceTxFields);
	}

	private Dictionary<string, object?> ReduceTxFields(Dictionary<string, object?> accumulator, Transaction item)
	{
		// Get field changes from tx.
		IEnumerable<Transaction.Change>? fieldChanges = item.Changes
			.Where((x) => x.FieldChange is not null);

		// Get the field writes from the tx.
		IEnumerable<Transaction.FieldChange> txFieldWrites = fieldChanges
			.Where((x) => x.Action == Transaction.Action.Write)
			.Select((x) => x.FieldChange!);

		// Get the field deletions from the tx.
		IEnumerable<Transaction.FieldChange> txFieldDeletions = fieldChanges
			.Where((x) => x.Action == Transaction.Action.Delete)
			.Select((x) => x.FieldChange!);

		// Get the unchanged fields from the accumulator (previously calculated changes).
		IEnumerable<KeyValuePair<string, object?>>? unchanged = accumulator
			.Where((x) => !txFieldWrites.Any((write) => write!.Key == x.Key))
			.Where((x) => !txFieldDeletions.Any((write) => write!.Key == x.Key));

		// Add the field writes.
		IEnumerable<KeyValuePair<string, object?>>? txWrites = txFieldWrites
			.Select((x) => new KeyValuePair<string, object?>(x!.Key, x!.Value));

		// Return the newly accumulated value.
		return unchanged.Union(txWrites).ToDictionary();
	}
}