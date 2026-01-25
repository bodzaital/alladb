using System.Text.Json.Serialization;
using AllaDb.Exceptions;

namespace AllaDb;

/// <summary>Represents abstract properties that all <see cref="IConstraint"/> implementations should have.</summary>
public interface IConstraintBase
{
	/// <summary>Gets the unique identifier of this <see cref="IConstraint"/> implementation.</summary>
	string Id { get; }

	/// <summary>Contains a reference to the <see cref="Collection"/> this <see cref="IConstraint"/> implementation belongs to.</summary>
	Collection? Collection { get; }
}

/// <inheritdoc cref="IConstraintBase" />
public abstract class ConstraintBase : IConstraintBase
{
	/// <inheritdoc cref="IConstraintBase.Id" />
	public string Id { get; } = Guid.NewGuid().ToString();

	/// <inheritdoc cref="IConstraintBase.Collection" />
	[JsonIgnore]
	public Collection? Collection { get; internal set; }
}

/// <summary>Represents a validation constraint.</summary>
public interface IConstraint : IConstraintBase
{
	/// <summary>Method called when validating a new <see cref="Document"/> before it is inserted into the <see cref="Collection"/>. If a constraint is violated, throws a <see cref="ConstraintViolationException"/> with an appropriate message.</summary>
	/// <param name="fields">Fields of the new <see cref="Document"/>.</param>
	void ValidateNewDocument(Dictionary<string, object?> fields);

	/// <summary>Method called when validating a new value written to a field of a <see cref="Document"/>. If a constraint is violated, throws a <see cref="ConstraintViolationException"/> with an appropriate message.</summary>
	/// <param name="key">Key of the field being updated.</param>
	/// <param name="value">New value of the field being updated.</param>
	void ValidateFieldWrite(string key, object? value);

	/// <summary>Method called when validating a field being deleted from a <see cref="Document"/>. If a constraint is violated, throws a <see cref="ConstraintViolationException"/> with an appropriate message.</summary>
	/// <param name="key">Key of the field being deleted.</param>
	void ValidateFieldDelete(string key);
}

/// <inheritdoc cref="IConstraint" />
public sealed class Constraint(string key, Constraint.Type type, params List<object?> constraintValues) : ConstraintBase
{
	/// <summary>Built-in constraints.</summary>
	public enum Type
	{
		/// <summary>Specifies that the associated field is required in the collection.</summary>
		Required,

		/// <summary>Specifies that the associated field must be unique in the collection.</summary>
		Unique,

		/// <summary>Specifies a default value for the associated field, if the field does not exist.</summary>
		Default,

		/// <summary>Specifies that the value of the associated field must be one of the specified set.</summary>
		From,
	}
	
	[JsonInclude]
	internal string Key { get; set; } = key;
	
	[JsonInclude]
	internal Type ConstraintType { get; set; } = type;

	[JsonInclude]
	internal List<object?> Values { get; set; } = constraintValues;

	/// <inheritdoc cref="IConstraint.ValidateNewDocument(Dictionary{string, object?})" />
	public void ValidateNewDocument(Dictionary<string, object?> fields)
	{
		if (ConstraintType == Type.Required)
		{
			if (!fields.ContainsKey(Key)) throw new ConstraintViolationException(
				ConstraintViolationException.RequiredMissingViolation(Key)
			);
		}

		if (ConstraintType == Type.Unique)
		{
			bool exists = Collection!.Documents
				.Where((x) => x.HasField(Key))
				.Select((x) => x.GetField<object?>(Key))
				.Contains(fields[Key]);
			
			if (exists) throw new ConstraintViolationException(
				ConstraintViolationException.NonUniqueViolation(Key)
			);
		}

		if (ConstraintType == Type.Default)
		{
			if (!fields.TryAdd(Key, Values.First())) fields[Key] = Values.First();
		}

		if (ConstraintType == Type.From)
		{
			bool isFromList = Values.Contains(fields[Key]);

			if (!isFromList) throw new ConstraintViolationException(
				ConstraintViolationException.OutOfRangeViolation(Key)
			);
		}
	}

	/// <inheritdoc cref="IConstraint.ValidateFieldWrite(string, object?)" />
	public void ValidateFieldWrite(string key, object? value)
	{
		if (key != Key) return;

		if (ConstraintType == Type.Unique)
		{
			bool exists = Collection!.Documents
				.Where((x) => x.HasField(Key))
				.Select((x) => x.GetField<object?>(Key))
				.Contains(value);
			
			if (exists) throw new ConstraintViolationException(
				ConstraintViolationException.NonUniqueViolation(Key)
			);
		}

		if (ConstraintType == Type.From)
		{
			bool isFromList = Values.Contains(value);

			if (!isFromList) throw new ConstraintViolationException(
				ConstraintViolationException.OutOfRangeViolation(Key)
			);
		}
	}

	/// <inheritdoc cref="IConstraint.ValidateFieldDelete(string)" />
	public void ValidateFieldDelete(string key)
	{
		if (key != Key) return;

		if (ConstraintType == Type.Required) throw new ConstraintViolationException(
			ConstraintViolationException.RequiredMissingViolation(Key)
		);
	}
}