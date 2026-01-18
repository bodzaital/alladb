using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using AllaDb.Exceptions;
using Microsoft.Extensions.Options;

namespace AllaDb;

public class Alla : IAlla
{
	private readonly AllaOptions _options;

	private readonly JsonSerializerOptions _serializerOptions;

	[JsonInclude]
	private readonly List<Collection> Collections = [];

	/// <summary>Creates a new instance of <see cref="Alla"/> using the specified configuration of <see cref="AllaOptions"/>.</summary>
	/// <param name="options">A configuration of <see cref="AllaOptions"/> from the DI container.</param>
	public Alla(IOptions<AllaOptions> options)
	{
		ArgumentNullException.ThrowIfNull(options);
		_options = options.Value;

		_serializerOptions = new()
		{
			WriteIndented = _options.IsPrettyPrint,
		};

		if (_options.IsEnumStrings)
		{
			_serializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
		}

		EnsureCreated();
		Collections = Load();
	}

	/// <summary>Creates a new instance of <see cref="Alla"/> using the specified instance of <see cref="AllaOptions"/>.</summary>
	/// <param name="options">An instance of <see cref="AllaOptions"/>.</param>
	public Alla(AllaOptions options)
	{
		_options = options;

		_serializerOptions = new()
		{
			WriteIndented = _options.IsPrettyPrint,
		};

		if (_options.IsEnumStrings)
		{
			_serializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
		}

		EnsureCreated();
		Collections = Load();
	}

	/// <summary>Removes all <see cref="Collection"/>s from the database.</summary>
	public void DropDatabase() => Collections.Clear();

	/// <summary>Removes a <see cref="Collection"/> from the database.</summary>
	/// <param name="collectionName">Name of the <see cref="Collection"/> to remove.</param>
	public void DropCollection(string collectionName) => Collections
		.RemoveAll((x) => x.Name == collectionName);

	/// <summary>Gets or creates a reference to a <see cref="Collection"/>.</summary>
	/// <param name="collectionName">Name of the <see cref="Collection"/> to get.</param>
	/// <returns>The associated <see cref="Collection"/>.</returns>
	public Collection GetCollection(string collectionName)
	{
		Collection? collection = Collections.Find((x) => x.Name == collectionName);

		if (collection is null)
		{
			collection = new(_options, collectionName);
			Collections.Add(collection);
		}

		return collection;
	}

	/// <summary>Returns a <see cref="ReadOnlyCollection{Collection}"/> of the <see cref="Collection"/>s of the database.</summary>
	/// <returns>A <see cref="ReadOnlyCollection{Collection}"/> that can be used to iterate over the <see cref="Collection"/>s of the database.</returns>
	public ReadOnlyCollection<Collection> GetCollections() => new(Collections);

	/// <summary>Serializes the database to the data source file. If the database is in-memory only, throws <see cref="InvalidOperationException"/>. If a <see cref="Collection"/> has an open <see cref="Transaction"/>, throws <see cref="UnresolvedTransactionException"/>.</summary>
	public void Persist()
	{
		if (_options.DataSource == ":memory:") throw new InvalidOperationException(
			"Database cannot be persisted because it is in-memory only."
		);

		if (Collections.Any((x) => x.OpenTransaction is not null)) throw new UnresolvedTransactionException(
			"The transaction must be resolved before persisting the collection."
		);
		
		File.WriteAllText(
			_options.DataSource,
			JsonSerializer.Serialize(
				Collections.Where((x) => x.Documents.Count > 0),
				_serializerOptions
			)
		);
	}

	private List<Collection> Load() => JsonSerializer.Deserialize<List<Collection>>(File.ReadAllText(_options.DataSource))
		?? throw new IllegalDeserializationException();

	private void EnsureCreated()
	{
		if (File.Exists(_options.DataSource)) return;
		Persist();
	}
}