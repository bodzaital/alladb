using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using AllaDb.Exceptions;

namespace AllaDb;

/// <summary>Represents a collection of <see cref="Document"/>s.</summary>
public class Collection(AllaOptions options, string name)
{
	private readonly AllaOptions _options = options;

	/// <summary>Gets the unique name of this <see cref="Collection"/>.</summary>
	[JsonInclude]
	public string Name { get; set; } = name;

	[JsonInclude]
	internal List<Document> Documents { get; set; } = [];

	[JsonInclude]
	internal List<IConstraint> Constraints { get; set; } = [];

	internal Transaction? OpenTransaction { get; set; }

	/// <summary>Method that is called when deserializing the database to set the associated <see cref="Collection"/> references in <see cref="Document"/>s and <see cref="Constraint"/>s.</summary>
	[OnDeserialized]
	public void OnDeserialized()
	{
		Documents.ForEach((x) => x.Collection = this);
		SetCollectionToConstraints();
	}

	/// <summary>Assigns a range of <see cref="IConstraint"/>s to the <see cref="Collection"/>.</summary>
	/// <param name="constraints">The range of <see cref="IConstraint"/>s whose elements should be assigned to the <see cref="Collection"/>.</param>
	/// <returns>The created constraints.</returns>
	public List<IConstraint> CreateConstraints(params IEnumerable<IConstraint> constraints)
	{
		Constraints.AddRange(constraints);
		SetCollectionToConstraints();
		return Constraints;
	}

	public void DeleteConstraint(string constraintId)
	{
		IConstraint constraint = Constraints.FirstOrDefault((x) => x.Id == constraintId)
			?? throw new ArgumentOutOfRangeException(nameof(constraintId), constraintId);

		Constraints.Remove(constraint);
	}

	/// <summary>Removes all <see cref="Document"/>s from the <see cref="Collection"/>.</summary>
	public void Truncate()
	{
		ThrowIfRequiredTransactionMissing();

		if (OpenTransaction is not null) GetDocuments().ToList().ForEach(OpenTransaction.AddDocumentDeletion);
		else Documents.Clear();
	}

	/// <summary>Deletes the <see cref="Document"/> with the specified Id from the <see cref="Collection"/>. If the specified <see cref="Document"/> is not found, throws an <see cref="ArgumentOutOfRangeException"/>.</summary>
	/// <param name="documentId">The Id of the <see cref="Document"/> to delete.</param>
	public void DeleteDocument(string documentId)
	{
		ThrowIfRequiredTransactionMissing();

		Document document = GetDocuments().FirstOrDefault((x) => x.Id == documentId)
			?? throw new ArgumentOutOfRangeException(nameof(documentId), documentId);

		if (OpenTransaction is not null) OpenTransaction.AddDocumentDeletion(document);
		else Documents.Remove(document);
	}

	/// <summary>Adds a <see cref="Document"/> to the <see cref="Collection"/>.</summary>
	/// <param name="fields">The <see cref="Dictionary{string, object?}"/> whose elements are copied to the new <see cref="Document"/>.</param>
	/// <returns>The created <see cref="Document"/>.</returns>
	public Document CreateDocument(Dictionary<string, object?> fields)
	{
		ThrowIfRequiredTransactionMissing();
		Constraints.ForEach((x) => x.ValidateNewDocument(fields));
		
		Document document = new(fields) { Collection = this };
		
		if (OpenTransaction is not null) OpenTransaction.AddDocumentWrite(document);
		else Documents.Add(document);
		
		return document;
	}

	/// <summary>Gets the <see cref="Document"/> associated with the specified Id.</summary>
	/// <param name="documentId">The Id of the <see cref="Document"/> to get.</param>
	/// <returns>The <see cref="Document"/> associated with the specified Id. If the specified Id is not found, throws an <see cref="ArgumentOutOfRangeException"/>.</returns>
	public Document GetDocument(string documentId) => GetDocuments().FirstOrDefault((x) => x.Id == documentId)
		?? throw new ArgumentOutOfRangeException(nameof(documentId), documentId);

	/// <summary>Creates a <see cref="Transaction"/> on the <see cref="Collection"/>.</summary>
	/// <returns>The created <see cref="Transaction"/>. If the <see cref="Collection"/> already has a <see cref="Transaction"/> in-progress, throws <see cref="UnresolvedTransactionException"/>.</returns>
	public Transaction CreateTransaction()
	{
		if (OpenTransaction is not null) throw new UnresolvedTransactionException(
			"Resolve the transaction before opening a new transaction."
		);
		
		OpenTransaction = new(this);
		return OpenTransaction;
	}

	/// <summary>Returns a <see cref="ReadOnlyCollection{Document}"/> of the <see cref="Document"/>s of the <see cref="Collection"/>.</summary>
	/// <returns>A <see cref="ReadOnlyCollection{Document}"/> that can be used to iterate over the <see cref="Document"/>s of the <see cref="Collection"/>.</returns>
	public ReadOnlyCollection<Document> GetDocuments()
	{
		if (OpenTransaction is null) return new(Documents);

		IEnumerable<Document> notDeletedDocuments = Documents.Where((live) =>
		{
			return OpenTransaction.Documents(Transaction.ChangeAction.Deleted).Find((inTx) => inTx.Id == live.Id) is null;
		});

		return new([..notDeletedDocuments, ..OpenTransaction.Documents(Transaction.ChangeAction.Written)]);
	}

	internal void ThrowIfRequiredTransactionMissing()
	{
		if (!_options.AreTransactionsRequired || OpenTransaction is not null) return;

		throw new InvalidOperationException(
			"Transactions are required."
		);
	}

	private void SetCollectionToConstraints() => Constraints
		.Select((x) => (ConstraintBase)x).ToList()
		.ForEach((x) => x.Collection = this);
}