using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using AllaDb.Exceptions;

namespace AllaDb;

public class Collection(AllaOptions options, string name)
{
	private readonly AllaOptions _options = options;

	[JsonInclude]
	public string Name { get; set; } = name;

	[JsonInclude]
	internal List<Document> Documents { get; set; } = [];

	[JsonInclude]
	internal List<IConstraint> Constraints { get; set; } = [];

	internal Transaction? Transaction { get; set; }

	[OnDeserialized]
	public void OnDeserialized()
	{
		Documents.ForEach((x) => x.Collection = this);
		Constraints.ForEach((x) => x.SetCollection(this));
	}

	public void CreateConstraints(params List<IConstraint> constraints) => Constraints.AddRange(constraints);

	public void Truncate()
	{
		ThrowIfRequiredTransactionMissing();

		if (Transaction is not null) GetDocuments().ToList().ForEach(Transaction.AddDocumentDeletion);
		else Documents.Clear();
	}

	public void DeleteDocument(string documentId)
	{
		ThrowIfRequiredTransactionMissing();

		Document document = GetDocuments().FirstOrDefault((x) => x.Id == documentId)
			?? throw new ArgumentOutOfRangeException(nameof(documentId), documentId);

		if (Transaction is not null) Transaction.AddDocumentDeletion(document);
		else Documents.Remove(document);
	}

	public Document CreateDocument(Dictionary<string, object?> fields)
	{
		ThrowIfRequiredTransactionMissing();
		Constraints.ForEach((x) => x.ValidateNewDocument(fields));
		
		Document document = new(fields) { Collection = this };
		
		if (Transaction is not null) Transaction.AddDocumentWrite(document);
		else Documents.Add(document);
		
		return document;
	}

	public Document GetDocument(string documentId) => GetDocuments().FirstOrDefault((x) => x.Id == documentId)
		?? throw new ArgumentOutOfRangeException(nameof(documentId), documentId);

	public Transaction CreateTransaction()
	{
		if (Transaction is not null) throw new UnresolvedTransactionException(
			"Resolve the transaction before opening a new transaction."
		);
		
		Transaction = new(this);
		return Transaction;
	}

	public ReadOnlyCollection<Document> GetDocuments()
	{
		if (Transaction is null) return new(Documents);

		IEnumerable<Document> notDeletedDocuments = Documents.Where((live) =>
		{
			return Transaction.Documents(Transaction.ChangeAction.Deleted).Find((inTx) => inTx.Id == live.Id) is null;
		});

		return new([..notDeletedDocuments, ..Transaction.Documents(Transaction.ChangeAction.Written)]);
	}

	internal void ThrowIfRequiredTransactionMissing()
	{
		if (!_options.AreTransactionsRequired || Transaction is not null) return;

		throw new InvalidOperationException(
			"Transactions are required."
		);
	}
}