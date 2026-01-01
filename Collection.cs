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

	internal Transaction? Transaction { get; set; }

	[OnDeserialized]
	public void OnDeserialized() => Documents.ForEach((x) => x.Collection = this);

	public void Truncate()
	{
		ThrowIfRequiredTransactionMissing();

		if (Transaction is not null) Transaction.Deletions.AddRange(TxDocuments());
		else Documents.Clear();
	}

	public void DeleteDocument(string documentId)
	{
		ThrowIfRequiredTransactionMissing();

		Document document = TxDocuments().FirstOrDefault((x) => x.Id == documentId)
			?? throw new ArgumentOutOfRangeException(nameof(documentId), documentId);

		if (Transaction is not null) Transaction.Deletions.Add(document);
		else Documents.Remove(document);
	}

	public Document CreateDocument(Dictionary<string, object?> fields)
	{
		ThrowIfRequiredTransactionMissing();
		
		Document document = new(fields) { Collection = this };
		
		if (Transaction is not null) Transaction.Additions.Add(document);
		else Documents.Add(document);
		
		return document;
	}

	public Document GetDocument(string documentId) => TxDocuments().FirstOrDefault((x) => x.Id == documentId)
		?? throw new ArgumentOutOfRangeException(nameof(documentId), documentId);

	public Transaction CreateTransaction()
	{
		if (Transaction is not null) throw new UnresolvedTransactionException(
			"Resolve the transaction before opening a new transaction."
		);
		
		Transaction = new(this);
		return Transaction;
	}

	public ReadOnlyCollection<Document> GetDocuments() => new(TxDocuments());

	internal ReadOnlyCollection<Document> TxDocuments()
	{
		if (Transaction is null) return new(Documents);

		IEnumerable<Document> notDeletedDocuments = Documents.Where((live) =>
		{
			return Transaction.Deletions.Find((inTx) => inTx.Id == live.Id) is null;
		});

		return new([..notDeletedDocuments, ..Transaction.Additions]);
	}

	internal void ThrowIfRequiredTransactionMissing()
	{
		if (!_options.AreTransactionsRequired || Transaction is not null) return;

		throw new InvalidOperationException(
			"Transactions are required."
		);
	}
}