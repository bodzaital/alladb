using System.Collections;
using System.Text.Json.Serialization;

namespace AllaDb;

public class Collection
{
	public required string Name { get; set; }

	[JsonIgnore]
	public List<Transaction> Transactions = [];

	[JsonIgnore]
	public bool HasTransaction { get => Transactions.Count > 0; }

	public List<Document> Documents { get; set; } = [];

	public IEnumerator GetEnumerator() => GetDocuments().GetEnumerator();

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

	public void Add(Dictionary<string, object?> fields)
	{
		Document document = new()
		{
			Fields = fields,
			Collection = this,
		};
		
		if (!HasTransaction)
		{
			Documents.Add(document);
			return;
		}

		Transactions.Last().Changes.Add(new(
			Transaction.Action.Write,
			DocumentChange: document
		));
	}

	public void AddRange(IEnumerable<Dictionary<string, object?>> fields)
	{
		foreach (Dictionary<string, object?> field in fields) Add(field);
	}

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

	public Transaction CreateTransaction()
	{
		Transaction transaction = new(this);
		Transactions.Add(transaction);
		return transaction;
	}

	public List<Collection> Partition(int size)
	{
		List<Collection> partitions = [];

		int i = 0;
		while (i < Documents.Count)
		{
			int count = Documents.Count - i > size
				? size
				: Documents.Count - i;
			
			partitions.Add(new()
			{
				Name = Name,
				Documents = Documents.GetRange(i, count),
			});

			i += size;
		}

		return partitions;
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