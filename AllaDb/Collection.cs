using System.Collections;
using System.Text.Json.Serialization;

namespace AllaDb;

/// <summary>A collection of documents.</summary>
public class Collection
{
	/// <summary>Unique name of this collection.</summary>
	public required string Name { get; set; }

	internal List<Transaction> Transactions = [];

	/// <summary>Returns true if this collection has any unresolved transactions.</summary>
	[JsonIgnore]
	public bool HasTransaction { get => Transactions.Count > 0; }

	[JsonInclude]
	internal List<Document> Documents { get; set; } = [];

	/// <summary>Exposes the enumerator of the underlying list of documents.</summary>
	public IEnumerator GetEnumerator() => GetDocuments().GetEnumerator();

	/// <summary>Deletes all documents of this collection.</summary>
	public void Clear()
	{
		if (!HasTransaction)
		{
			Documents.Clear();
			return;
		}

		Documents.ForEach((document) => Transactions.Last().Changes.Add(new(
			Transaction.Action.Delete,
			DocumentChange: document
		)));
	}

	/// <summary>Adds the specified fields to a new document in this collection and returns the created document.</summary>
	public Document Add(Dictionary<string, object?> fields)
	{
		Document document = new()
		{
			Fields = fields,
			Collection = this,
		};
		
		if (!HasTransaction)
		{
			Documents.Add(document);
		}
		else
		{
			Transactions.Last().Changes.Add(new(
				Transaction.Action.Write,
				DocumentChange: document
			));
		}

		return document;
	}

	/// <summary>Adds the specified list of fields to new documents and returns the created documents.</summary>
	public List<Document> AddRange(IEnumerable<Dictionary<string, object?>> fields)
	{
		return [.. fields.Select(Add)];
	}

	/// <summary>Removes the specific object from the collection.</summary>
	public void Remove(Document document)
	{
		if (!HasTransaction)
		{
			Documents.Remove(document);
			return;
		}

		Transactions.Last().Changes.Add(new(
			Transaction.Action.Delete,
			DocumentChange: document
		));
	}

	/// <summary>Deletes all documents from the collection that match the conditions defined by the specified predicate</summary>
	public void RemoveAll(Func<Document, bool> predicate)
	{
		if (!HasTransaction)
		{
			Documents.RemoveAll(new(predicate));
			return;
		}

		Documents.Where(predicate).ToList().ForEach((document) => Transactions.Last().Changes.Add(new(
			Transaction.Action.Delete,
			DocumentChange: document
		)));
	}

	/// <summary>Adds a new transaction over this collection to this collection's stack.</summary>
	public Transaction CreateTransaction()
	{
		Transaction transaction = new(this);
		Transactions.Add(transaction);
		return transaction;
	}

	private List<Document> GetDocuments()
	{
		if (!HasTransaction) return Documents;

		return Transactions.Aggregate(new List<Document>(Documents), ReduceTxDocuments);
	}

	private List<Document> ReduceTxDocuments(List<Document> accumulator, Transaction item)
	{
		IEnumerable<Document> unchanged = accumulator.Where((doc) => !item.Changes
			.Any((change) => change.DocumentChange?.Id == doc.Id)
		);

		IEnumerable<Document> writtenDocuments = item.Changes
			.Where((x) => x.Action == Transaction.Action.Write)
			.Where((x) => x.DocumentChange is not null)
			.Select((x) => x.DocumentChange!);

		return [.. unchanged.Union(writtenDocuments)];
	}
}