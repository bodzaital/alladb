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

	public enum FieldChangeAction
	{
		Written,
		Deleted,
	}

	public record FieldChange(
		string DocumentId,
		string Key,
		object? Value,
		FieldChangeAction Action
	);

	internal Collection Collection { get; set; } = collection;

	internal ResolutionAction Resolution { get; set; } = ResolutionAction.Unresolved;

	internal List<Document> Additions { get; set; } = [];

	internal List<Document> Deletions { get; set; } = [];

	internal List<FieldChange> FieldChanges { get; set; } = [];

	public void Dispose()
	{
		GC.SuppressFinalize(this);

		if (Resolution == ResolutionAction.Unresolved) throw new UnresolvedTransactionException(
			"The transaction must be resolved before control leaves the block of the using statement."
		);

		if (Resolution == ResolutionAction.Commit)
		{
			Deletions.ForEach((x) => Collection.Documents.Remove(x));
			Additions.ForEach(Collection.Documents.Add);

			FieldChanges.ForEach((fieldChange) =>
			{
				Document document = Collection.Documents.First((document) => document.Id == fieldChange.DocumentId);
				if (fieldChange.Action == FieldChangeAction.Written) document.Fields[fieldChange.Key] = fieldChange.Value;
				if (fieldChange.Action == FieldChangeAction.Deleted) document.Fields.Remove(fieldChange.Key);
			});
		}

		Collection.Transaction = null;
	}

	public void MarkForCommit() => Resolution = ResolutionAction.Commit;

	public void MarkForRollback() => Resolution = ResolutionAction.Rollback;
}