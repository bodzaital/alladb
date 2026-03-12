#pragma warning disable CA1816

namespace AllaDb;

public class Transaction(Collection collection) : IDisposable
{
	public enum Resolution
	{
		Committed,
		RolledBack,
	}

	public enum Action
	{
		Write,
		Delete,
	}

	public record FieldChange(
		string Id,
		string Key,
		object? Value
	);

	public record Change(
		Action Action,
		FieldChange? FieldChange = null,
		Document? DocumentChange = null
	);

	public Resolution Result = Resolution.Committed;

	public List<Change> Changes { get; set; } = [];

	public void Dispose()
	{
		if (Result == Resolution.Committed)
		{
			Changes.ForEach(CommitChange);
		}

		collection.Transactions.Remove(this);
	}

	public void Commit()
	{
		Result = Resolution.Committed;
	}

	public void Rollback()
	{
		Result = Resolution.RolledBack;
	}

	private void CommitChange(Change change)
	{
		CommitFieldChange(change.Action, change.FieldChange);
		CommitDocumentChange(change.Action, change.DocumentChange);
	}

	private void CommitFieldChange(Action action, FieldChange? fieldChange)
	{
		if (fieldChange is null) return;

		Document document = collection.Documents.First((x) => x.Id == fieldChange.Id);

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
			collection.Documents.Add(document);
		}

		if (action == Action.Delete)
		{
			collection.Documents.Remove(document);
		}
	}
}