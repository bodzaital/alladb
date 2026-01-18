using AllaDb.Exceptions;

namespace AllaDb;

/// <summary>Represents a transaction.</summary>
public class Transaction(Collection collection) : IDisposable
{
	internal enum ResolutionAction
	{
		Unresolved,
		Commit,
		Rollback,
	}

	internal enum ChangeAction
	{
		Written,
		Deleted,
	}

	internal record FieldChange(
		string DocumentId,
		string Key,
		object? Value,
		ChangeAction Action
	);

	internal record DocumentChange(
		Document Document,
		ChangeAction Action
	);

	internal readonly List<DocumentChange> DocumentChanges = [];

	internal readonly List<FieldChange> FieldChanges = [];

	internal Collection Collection { get; set; } = collection;

	internal ResolutionAction Resolution { get; set; } = ResolutionAction.Unresolved;

	/// <summary>Finalize and complete the <see cref="Transaction"/>. If the <see cref="Transaction"/> was neither marked for commit nor rollback, throws an <see cref="UnresolvedTransactionException"/>.</summary>
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

			GetFieldChanges(ChangeAction.Written).ForEach((fieldChange) =>
			{
				Document document = Collection.Documents.First((document) => document.Id == fieldChange.DocumentId);
				document.Fields[fieldChange.Key] = fieldChange.Value;
			});

			GetFieldChanges(ChangeAction.Deleted).ForEach((fieldChange) =>
			{
				Document document = Collection.Documents.First((document) => document.Id == fieldChange.DocumentId);
				document.Fields.Remove(fieldChange.Key);
			});
		}

		Collection.OpenTransaction = null;
	}

	/// <summary>Marks the <see cref="Transaction"/> to commit (apply) all changes.</summary>
	public void MarkForCommit() => Resolution = ResolutionAction.Commit;

	/// <summary>Marks the <see cref="Transaction"/> to roll back (abort) the changes.</summary>
	public void MarkForRollback() => Resolution = ResolutionAction.Rollback;

	internal void AddDocumentDeletion(Document document) => DocumentChanges.Add(new(document, ChangeAction.Deleted));

	internal void AddDocumentWrite(Document document) => DocumentChanges.Add(new(document, ChangeAction.Written));

	internal List<Document> Documents(ChangeAction? action = null) => [.. DocumentChanges
		.Where((x) => x.Action == action)
		.Select((x) => x.Document)
	];

	internal void AddFieldDeletion(string documentId, string key) => FieldChanges.Add(new(
		documentId,
		key,
		null,
		ChangeAction.Deleted
	));

	internal void AddFieldWrite(string documentId, string key, object? value) => FieldChanges.Add(new(
		documentId,
		key,
		value,
		ChangeAction.Written
	));

	internal List<FieldChange> GetFieldChanges(ChangeAction action) => [.. FieldChanges
		.Where((x) => x.Action == action)
	];
}