using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace AllaDb;

/// <summary>A collection of documents.</summary>
public class Collection
{
	/// <summary>Unique name of this collection.</summary>
	public required string Name { get; set; }

	internal Stack<Transaction> Transactions = [];

	/// <summary>Returns true if this collection has any unresolved transactions.</summary>
	[JsonIgnore]
	public bool HasTransactions { get => Transactions.Count > 0; }

	[JsonInclude]
	internal List<Document> Documents { get; set; } = [];

	/// <summary>Exposes the enumerator of the underlying list of documents.</summary>
	public IEnumerator GetEnumerator() => GetDocuments().GetEnumerator();

	/// <summary>Removes all documents from the collection.</summary>
	public void Clear()
	{
		if (!HasTransactions)
		{
			Documents.Clear();
			return;
		}

		Documents.ForEach((document) => Transactions.Last().Changes.Add(new(
			Transaction.Action.Delete,
			DocumentChange: document
		)));
	}

	/// <summary>Adds a new document with the specified fields to the end of the collection.</summary>
	/// <param name="fields">The fields of a new document to be added to the end of the collection.</param>
	/// <returns>The new document in the collection.</returns>
	public Document Add(Dictionary<string, object?> fields)
	{
		Document document = new()
		{
			Fields = fields,
			Collection = this,
		};
		
		if (!HasTransactions)
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

	/// <summary>Adds the specified list of fields to the end of the collection.</summary>
	/// <param name="fields">The list of fields that should be added to the end of the collection.</param>
	/// <returns>The list of new documents in the collection.</returns>
	public List<Document> AddRange(IEnumerable<Dictionary<string, object?>> fields)
	{
		return [.. fields.Select(Add)];
	}

	/// <summary>Removes the specific document from the collection.</summary>
	/// <param name="document">The document to remove from the collection.</param>
	public void Remove(Document document)
	{
		if (!HasTransactions)
		{
			Documents.Remove(document);
			return;
		}

		Transactions.Last().Changes.Add(new(
			Transaction.Action.Delete,
			DocumentChange: document
		));
	}

	/// <summary>Removes all document that match the condition defined by the specified delegate.</summary>
	/// <param name="predicate">The function delegate that defines the condition of the documents to remove.</param>
	public void RemoveAll(Func<Document, bool> predicate)
	{
		if (!HasTransactions)
		{
			Documents.RemoveAll(new(predicate));
			return;
		}

		Documents.Where(predicate).ToList().ForEach((document) => Transactions.Last().Changes.Add(new(
			Transaction.Action.Delete,
			DocumentChange: document
		)));
	}

	/// <summary>Gets the document associated with the specified ID.</summary>
	/// <param name="id">The ID of the document to get.</param>
	/// <returns>The document if the collection contains it by ID; otherwise, null.</returns>
	public Document? GetDocument(string id)
	{
		return GetDocuments().Find((x) => x.Id == id);
	}

	/// <summary>Gets the document associated with the specified ID.</summary>
	/// <param name="id">The ID of the document to get.</param>
	/// <param name="document">When this method returns, contains the document associated with the specified ID, if the ID is found; otherwise, null. This parameter is passed uninitialized.</param>
	/// <returns>true if the collection contains a document with the specified ID; otherwise, false.</returns>
	public bool TryGetDocument(string id, [NotNullWhen(true)] out Document? document)
	{
		document = GetDocuments().Find((x) => x.Id == id);
		return document is not null;
	}

	/// <summary>Adds a new transaction over this collection to the end of the transaction stack.</summary>
	/// <returns>The new transaction over this collection.</returns>
	public Transaction CreateTransaction()
	{
		Transaction transaction = new(this);
		Transactions.Push(transaction);
		return transaction;
	}

	/// <summary>Get all documents of the collection.</summary>
	/// <returns>All documents of the collection.</returns>
	public List<Document> GetDocuments()
	{
		if (!HasTransactions) return Documents;

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