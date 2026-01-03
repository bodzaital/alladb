using AllaDb.Exceptions;

namespace AllaDb;

public class Transaction(Collection collection) : IDisposable
{
	public enum ResolutionAction
	{
		Unresolved,
		Commit,
		Rollback,
	}

	public enum ChangeAction
	{
		Written,
		Deleted,
	}

	public record FieldChange(
		string DocumentId,
		string Key,
		object? Value,
		ChangeAction Action
	);

	public record DocumentChange(
		Document Document,
		ChangeAction Action
	);

	private readonly List<DocumentChange> _documentChanges = [];

	private readonly List<FieldChange> _fieldChanges = [];

	internal Collection Collection { get; set; } = collection;

	internal ResolutionAction Resolution { get; set; } = ResolutionAction.Unresolved;

	public void Dispose()
	{
		GC.SuppressFinalize(this);

		if (Resolution == ResolutionAction.Unresolved) throw new UnresolvedTransactionException(
			"The transaction must be resolved before it is disposed."
		);

		if (Resolution == ResolutionAction.Commit)
		{
			Documents(ChangeAction.Deleted).ForEach((x) => Collection.Documents.Remove(x));
			Documents(ChangeAction.Written).ForEach(Collection.Documents.Add);

			FieldChanges(ChangeAction.Written).ForEach((fieldChange) =>
			{
				Document document = Collection.Documents.First((document) => document.Id == fieldChange.DocumentId);
				document.Fields[fieldChange.Key] = fieldChange.Value;
			});

			FieldChanges(ChangeAction.Deleted).ForEach((fieldChange) =>
			{
				Document document = Collection.Documents.First((document) => document.Id == fieldChange.DocumentId);
				document.Fields.Remove(fieldChange.Key);
			});
		}

		Collection.Transaction = null;
	}

	public void MarkForCommit() => Resolution = ResolutionAction.Commit;

	public void MarkForRollback() => Resolution = ResolutionAction.Rollback;

	internal void AddDocumentDeletion(Document document) => _documentChanges.Add(new(document, ChangeAction.Deleted));

	internal void AddDocumentWrite(Document document) => _documentChanges.Add(new(document, ChangeAction.Written));

	internal List<Document> Documents(ChangeAction? action = null) => [.. _documentChanges
		.Where((x) => x.Action == action)
		.Select((x) => x.Document)
	];

	internal void AddFieldDeletion(string documentId, string key) => _fieldChanges.Add(new(
		documentId,
		key,
		null,
		ChangeAction.Deleted
	));

	internal void AddFieldWrite(string documentId, string key, object? value) => _fieldChanges.Add(new(
		documentId,
		key,
		value,
		ChangeAction.Written
	));

	internal List<FieldChange> FieldChanges(ChangeAction action) => [.. _fieldChanges
		.Where((x) => x.Action == action)
	];
}