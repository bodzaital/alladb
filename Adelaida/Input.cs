using System.Text;

namespace Adelaida;

public static class Input
{
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

	private static StringBuilder? _stringBuilder;
	private static List<string> _completionSource = [];
	private static Queue<string> _history { get; set; } = [];

	public static void Setup(List<string> completionSource)
	{
		_completionSource = completionSource;
	}

	public static void WriteHistory()
	{
		_history.ToList().ForEach(Console.WriteLine);
	}

	public static string ReadLine()
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
			else if (ShouldArrow(keyInfo.Key)) HandleArrow(keyInfo.Key);
		} while (keyInfo.Key != ConsoleKey.Enter);

		string input = _stringBuilder.ToString();
		
		_history.Enqueue(input);
		if (_history.Count > 10) _history.Dequeue();
		
		return input;
	}

	private static void HandleArrow(ConsoleKey key)
	{
		if (key == ConsoleKey.UpArrow)
		{
			if (_history.Count > 0)
			{
				HandleBackspace(_stringBuilder!.Length);
				_history.Last().ToList().ForEach(HandleAlphanumeric);
			}
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

		length.Times(() => MoveConsoleCursor(-1, 0));
		length.Times(() => Console.Write(" "));
		length.Times(() => MoveConsoleCursor(-1, 0));
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
		HandleAlphanumeric(autocompletedOption.First() + " ");
	}

	private static void RemoveLastKeys(int length = 1)
	{
		length.Times(() => _stringBuilder = _stringBuilder!.Remove(_stringBuilder.Length - 1, 1));
	}

	private static void MoveConsoleCursor(int dx, int dy) => Console.SetCursorPosition(
		Console.GetCursorPosition().Left + dx,
		Console.GetCursorPosition().Top + dy
	);
}