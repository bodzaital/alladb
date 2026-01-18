using System.Collections.ObjectModel;
using NSubstitute;

namespace AllaDb.Test.DocumentTests;

[TestFixture]
public class DocumentTest
{
	[Test]
	public void GetFields_WithNoTx_ReturnsHardFields()
	{
		Document document = new(new()
		{
			{ "key", "value" }
		})
		{
			Collection = new Collection(new(), "name"),
		};

		ReadOnlyDictionary<string, object?> result = document.GetFields();

		Assert.That(result, Is.Not.Empty);
		Assert.That(result, Has.Count.EqualTo(1));
		AssertHasKeyValue(result, "key", "value");
	}

	[Test]
	public void GetFields_WithTx_ReturnsSoftFields()
	{
		Collection collection = new(new(), "name");
		Transaction transaction = new(collection);
		collection.OpenTransaction = transaction;

		Document document = new(new()
		{
			{ "keyToKeep", "valueToKeep" },
			{ "keyToDelete", "valueToDelete" },
			{ "keyToUpdate", "valueToUpdate" },
		})
		{
			Collection = collection,
		};

		transaction.FieldChanges.Add(new(
			document.Id,
			"keyToDelete",
			null,
			Transaction.ChangeAction.Deleted
		));

		transaction.FieldChanges.Add(new(
			document.Id,
			"keyToUpdate",
			"updatedValue",
			Transaction.ChangeAction.Written
		));

		transaction.FieldChanges.Add(new(
			document.Id,
			"keyToAdd",
			"valueToAdd",
			Transaction.ChangeAction.Written
		));

		ReadOnlyDictionary<string, object?> result = document.GetFields();

		Assert.That(result, Is.Not.Empty);
		Assert.That(result, Has.Count.EqualTo(3));
		AssertHasKeysValues(
			new(result, "keyToKeep", "valueToKeep"),
			new(result, "keyToUpdate", "updatedValue"),
			new(result, "keyToAdd", "valueToAdd")
		);
	}

	[Test]
	public void HasField_ReturnsTrue()
	{
		Document document = new(new()
		{
			{ "key", "value" }
		})
		{
			Collection = new Collection(new(), "name"),
		};

		bool result = document.HasField("key");

		Assert.That(result, Is.True);
	}

	[Test]
	public void HasField_ReturnsFalse()
	{
		Document document = new(new()
		{
			{ "key", "value" }
		})
		{
			Collection = new Collection(new(), "name"),
		};

		bool result = document.HasField("key2");

		Assert.That(result, Is.False);
	}

	[Test]
	public void GetField_ThrowsIfHasNoField()
	{
		Document document = new(new()
		{
			{ "key", "value" }
		})
		{
			Collection = new Collection(new(), "name"),
		};

		Assert.Throws<ArgumentOutOfRangeException>(() => document.GetField<string>("key2"));
	}

	[Test]
	public void GetField_ReturnsFieldValue()
	{
		Document document = new(new()
		{
			{ "key", "value" }
		})
		{
			Collection = new Collection(new(), "name"),
		};

		string? result = document.GetField<string>("key");

		Assert.That(result, Is.Not.Null);
		Assert.That(result, Is.EqualTo("value"));
	}

	[Test]
	public void TryGetField_ReturnsTrue()
	{
		Document document = new(new()
		{
			{ "key", "value" }
		})
		{
			Collection = new Collection(new(), "name"),
		};

		bool result = document.TryGetField("key", out string? resultValue);

		Assert.That(result, Is.True);
		Assert.That(resultValue, Is.Not.Null);
		Assert.That(resultValue, Is.EqualTo("value"));
	}

	[Test]
	public void TryGetField_ReturnsFalse()
	{
		Document document = new(new()
		{
			{ "key", "value" }
		})
		{
			Collection = new Collection(new(), "name"),
		};

		bool result = document.TryGetField("key2", out string? resultValue);

		Assert.That(result, Is.False);
		Assert.That(resultValue, Is.Null);
	}

	[Test]
	public void SetField_ThrowsIfRequiredTxMissing()
	{
		Document document = new(new()
		{
			{ "key", "value" }
		})
		{
			Collection = new Collection(new()
			{
				AreTransactionsRequired = true,
			}, "name"),
		};

		Assert.Throws<InvalidOperationException>(() => document.SetField("key", "newValue"));
	}

	[Test]
	public void SetField_DoesNotThrowWithRequiredTx()
	{
		Collection collection = new(new()
		{
			AreTransactionsRequired = true,
		}, "name");

		Document document = new(new()
		{
			{ "key", "value" }
		})
		{
			Collection = collection,
		};

		_ = collection.CreateTransaction();

		document.SetField("key", "newValue");
	}

	[Test]
	public void SetField_UpdatesHardField()
	{
		Collection collection = new(new(), "name");
		IConstraint constraintMock = Substitute.For<IConstraint>();
		collection.Constraints.Add(constraintMock);

		Document document = new(new()
		{
			{ "key", "value" }
		})
		{
			Collection = collection,
		};

		document.SetField("key", "newValue");

		Assert.That(document.Fields, Has.Count.EqualTo(1));
		AssertHasKeyValue(document.Fields, "key", "newValue");

		constraintMock.Received().ValidateFieldWrite("key", "newValue");
	}

	[Test]
	public void SetField_AddsHardField()
	{
		Collection collection = new(new(), "name");
		IConstraint constraintMock = Substitute.For<IConstraint>();
		collection.Constraints.Add(constraintMock);

		Document document = new(new()
		{
			{ "key", "value" }
		})
		{
			Collection = collection,
		};

		document.SetField("key2", "value2");

		Assert.That(document.Fields, Has.Count.EqualTo(2));
		AssertHasKeyValue(document.Fields, "key", "value");
		AssertHasKeyValue(document.Fields, "key2", "value2");

		constraintMock.Received().ValidateFieldWrite("key2", "value2");
	}

	[Test]
	public void SetField_AddsOrUpdatesSoftField()
	{
		Collection collection = new(new(), "name");
		IConstraint constraintMock = Substitute.For<IConstraint>();
		collection.Constraints.Add(constraintMock);

		Transaction transaction = new(collection);
		collection.OpenTransaction = transaction;

		Document document = new(new()
		{
			{ "key", "value" }
		})
		{
			Collection = collection,
		};

		document.SetField("key", "newValue");
		document.SetField("key2", "value2");

		Assert.That(document.Fields, Has.Count.EqualTo(1));
		AssertHasKeyValue(document.Fields, "key", "value");

		Assert.That(transaction.FieldChanges, Has.Count.EqualTo(2));
		
		Transaction.FieldChange? updateChange = transaction.FieldChanges.Find((x) => x.Key == "key");
		Assert.That(updateChange, Is.Not.Null);
		Assert.That(updateChange.Value, Is.EqualTo("newValue"));
		Assert.That(updateChange.Action, Is.EqualTo(Transaction.ChangeAction.Written));

		Transaction.FieldChange? addChange = transaction.FieldChanges.Find((x) => x.Key == "key2");
		Assert.That(addChange, Is.Not.Null);
		Assert.That(addChange.Value, Is.EqualTo("value2"));
		Assert.That(updateChange.Action, Is.EqualTo(Transaction.ChangeAction.Written));

		constraintMock.Received().ValidateFieldWrite("key", "newValue");
		constraintMock.Received().ValidateFieldWrite("key2", "value2");
	}

	[Test]
	public void DeleteField_ThrowsIfRequiredTxMissing()
	{
		Document document = new(new()
		{
			{ "key", "value" }
		})
		{
			Collection = new Collection(new()
			{
				AreTransactionsRequired = true,
			}, "name"),
		};

		Assert.Throws<InvalidOperationException>(() => document.DeleteField("key"));
	}

	[Test]
	public void DeleteField_DoesNotThrowWithRequiredTx()
	{
		Collection collection = new(new()
		{
			AreTransactionsRequired = true,
		}, "name");

		Document document = new(new()
		{
			{ "key", "value" }
		})
		{
			Collection = collection,
		};

		_ = collection.CreateTransaction();

		document.DeleteField("key");
	}

	[Test]
	public void DeleteField_ThrowsIfKeyIsMissing()
	{
		Document document = new(new()
		{
			{ "key", "value" }
		})
		{
			Collection = new(new(), "name"),
		};

		Assert.Throws<ArgumentOutOfRangeException>(() => document.DeleteField("key2"));
	}

	[Test]
	public void DeleteField_HardDeletes()
	{
		Collection collection = new(new(), "name");
		IConstraint constraintMock = Substitute.For<IConstraint>();
		collection.Constraints.Add(constraintMock);

		Document document = new(new()
		{
			{ "key", "value" },
			{ "key2", "value2" }
		})
		{
			Collection = collection,
		};

		document.DeleteField("key2");

		Assert.That(document.Fields, Has.Count.EqualTo(1));
		AssertHasKeyValue(document.Fields, "key", "value");
		Assert.That(document.Fields, Does.Not.ContainKey("key2"));

		constraintMock.Received().ValidateFieldDelete("key2");
	}

	[Test]
	public void DeleteField_SoftDeletes()
	{
		Collection collection = new(new(), "name");
		IConstraint constraintMock = Substitute.For<IConstraint>();
		collection.Constraints.Add(constraintMock);

		Transaction transaction = new(collection);
		collection.OpenTransaction = transaction;

		Document document = new(new()
		{
			{ "key", "value" },
			{ "key2", "value2" }
		})
		{
			Collection = collection,
		};

		document.DeleteField("key2");

		Assert.That(document.Fields, Has.Count.EqualTo(2));
		AssertHasKeyValue(document.Fields, "key", "value");
		AssertHasKeyValue(document.Fields, "key2", "value2");
		
		Assert.That(transaction.FieldChanges, Has.Count.EqualTo(1));
		
		Transaction.FieldChange? updateChange = transaction.FieldChanges.Find((x) => x.Key == "key2");
		Assert.That(updateChange, Is.Not.Null);
		Assert.That(updateChange.Key, Is.EqualTo("key2"));
		Assert.That(updateChange.Action, Is.EqualTo(Transaction.ChangeAction.Deleted));

		constraintMock.Received().ValidateFieldDelete("key2");
	}

	private static void AssertHasKeyValue(IDictionary<string, object?> result, string key, object? value)
	{
		Assert.That(result, Contains.Key(key));
		Assert.That(result[key], Is.EqualTo(value));
	}

	private static void AssertHasKeysValues(params List<KeyValueTest> tests)
	{
		tests.ForEach((test) =>
		{
			Assert.That(test.Result, Contains.Key(test.Key));
			Assert.That(test.Result[test.Key], Is.EqualTo(test.Value));
		});
	}
}

public record KeyValueTest(IDictionary<string, object?> Result, string Key, object? Value);