#pragma warning disable CA1816

namespace AllaDb;

/// <summary>Collection and document level transactions.</summary>
public class Transaction : IDisposable
{
	internal enum Resolution
	{
		Committed,
		RolledBack,
	}

	internal enum Action
	{
		Write,
		Delete,
	}

	internal record FieldChange(
		string Id,
		string Key,
		object? Value
	);

	internal record Change(
		Action Action,
		FieldChange? FieldChange = null,
		Document? DocumentChange = null
	);

	private readonly Collection _collection;

	internal Resolution Result = Resolution.Committed;

	internal List<Change> Changes { get; set; } = [];

	internal Transaction(Collection collection)
	{
		_collection = collection;
	}

	/// <summary>Resolves the transaction. If unmarked, the default resolution is to commit.</summary>
	public void Dispose()
	{
		if (Result == Resolution.Committed)
		{
			Changes.ForEach(CommitChange);
		}
		
		_collection.Transactions.Remove(this);
	}

	/// <summary>Marks this transaction to be committed. This is the default resolution.</summary>
	public Transaction Commit()
	{
		Result = Resolution.Committed;
		return this;
	}

	/// <summary>Marks this transaction to be rolled back.</summary>
	public Transaction Rollback()
	{
		Result = Resolution.RolledBack;
		return this;
	}

	private void CommitChange(Change change)
	{
		CommitFieldChange(change.Action, change.FieldChange);
		CommitDocumentChange(change.Action, change.DocumentChange);
	}

	private void CommitFieldChange(Action action, FieldChange? fieldChange)
	{
		if (fieldChange is null) return;

		Document document = _collection.Documents.First((x) => x.Id == fieldChange.Id);

		if (action == Action.Write)
		{
			document.Fields[fieldChange.Key] = fieldChange.Value;
		}

		if (action == Action.Delete)
		{
			document.Fields.Remove(fieldChange.Key);
		}
	}

	private void CommitDocumentChange(Action action, Document? document)
	{
		if (document is null) return;

		if (action == Action.Write)
		{
			_collection.Documents.Add(document);
		}

		if (action == Action.Delete)
		{
			_collection.Documents.Remove(document);
		}
	}
}