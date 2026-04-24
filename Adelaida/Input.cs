using System.Text;

namespace Adelaida;

public static class Input
{
	private static readonly int _maxQueue = 10;

	private static readonly ConsoleKey[] _arrowKeys = [
		ConsoleKey.UpArrow,
		ConsoleKey.RightArrow,
		ConsoleKey.DownArrow,
		ConsoleKey.LeftArrow,
	];

	private static readonly ConsoleKey[] _controlKeys = [
		ConsoleKey.Tab,
		ConsoleKey.Enter,
		ConsoleKey.Backspace,
		.._arrowKeys,
	];

	private static readonly Queue<string> _inputs = [];

	private static StringBuilder? _stringBuilder;
	private static List<string> _completionSource = [];

	public static void Setup(List<string> completionSource)
	{
		_completionSource = completionSource;
	}

	public static string ReadLine(Queue<string> history)
	{
		_stringBuilder = new();

		ConsoleKeyInfo keyInfo;

		do
		{
			keyInfo = Console.ReadKey(true);

			if (IsAlphanumeric(keyInfo.Key)) HandleAlphanumeric(keyInfo.KeyChar);
			else if (ShouldEnter(keyInfo.Key)) HandleEnter();
			else if (ShouldBackspace(keyInfo.Key) && CanBackspace()) HandleBackspace();
			else if (ShouldTab(keyInfo.Key)) HandleTab();
			else if (ShouldArrow(keyInfo.Key)) HandleUpArrow(history);
		} while (keyInfo.Key != ConsoleKey.Enter);

		string input = _stringBuilder.ToString();
		_inputs.Enqueue(input);

		if (_inputs.Count > _maxQueue) _inputs.Dequeue();
		
		return input;
	}

	private static void HandleUpArrow(Queue<string> history)
	{
		if (history.Count > 0)
		{
			HandleBackspace(_stringBuilder!.Length);
			history.Last().ToList().ForEach(HandleAlphanumeric);
		}
	}

	private static bool ShouldArrow(ConsoleKey key) =>
		_arrowKeys.Contains(key);

	private static bool IsAlphanumeric(ConsoleKey key) =>
		!_controlKeys.Contains(key);

	private static bool ShouldEnter(ConsoleKey key) =>
		key == ConsoleKey.Enter;

	private static bool ShouldBackspace(ConsoleKey key) =>
		key == ConsoleKey.Backspace;

	private static bool ShouldTab(ConsoleKey key) =>
		key == ConsoleKey.Tab;

	private static bool CanBackspace() =>
		_stringBuilder!.Length > 0;

	private static void HandleAlphanumeric(string text)
	{
		Console.Write(text);
		_stringBuilder!.Append(text);
	}

	private static void HandleAlphanumeric(char character)
	{
		HandleAlphanumeric(character.ToString());
	}

	private static void HandleEnter()
	{
		Console.WriteLine();
	}

	private static void HandleBackspace(int length = 1)
	{
		RemoveLastKeys(length);

		length.Times((_) => MoveConsoleCursor(-1, 0));
		length.Times((_) => Console.Write(" "));
		length.Times((_) => MoveConsoleCursor(-1, 0));
	}

	private static void HandleTab()
	{
		string partialInput = _stringBuilder!.ToString();
		List<string> autocompletedOption = [.. _completionSource
			.Where((x) => x.StartsWith(partialInput, StringComparison.InvariantCultureIgnoreCase))
			.Order()
		];

		if (autocompletedOption.Count == 0) return;

		HandleBackspace(_stringBuilder.Length);
		HandleAlphanumeric(autocompletedOption.First());
	}

	private static void RemoveLastKeys(int length = 1)
	{
		length.Times((_) => _stringBuilder = _stringBuilder!.Remove(_stringBuilder.Length - 1, 1));
	}

	private static void MoveConsoleCursor(int dx, int dy) => Console.SetCursorPosition(
		Console.GetCursorPosition().Left + dx,
		Console.GetCursorPosition().Top + dy
	);
}