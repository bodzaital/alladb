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
	private static int _historyCursor = 0;

	public static void Setup(List<string> completionSource)
	{
		_completionSource = completionSource;
	}

	public static void WriteHistory()
	{
		_history.ToList().ForEach(Output.WriteLine);
	}

	public static string ReadLine()
	{
		_stringBuilder = new();
		_historyCursor = 0;

		ConsoleKeyInfo keyInfo;

		do
		{
			keyInfo = Console.ReadKey(true);

			if (IsAlphanumeric(keyInfo.Key)) HandleAlphanumeric(keyInfo.KeyChar);
			else if (IsEnter(keyInfo.Key)) HandleEnter();
			else if (IsBackspace(keyInfo.Key) && CanBackspace()) HandleBackspace();
			else if (IsTab(keyInfo.Key)) HandleTab();
			else if (IsArrow(keyInfo.Key)) HandleArrow(keyInfo.Key);
		} while (keyInfo.Key != ConsoleKey.Enter);

		string input = _stringBuilder.ToString();
		
		_history.Enqueue(input);
		if (_history.Count > 10) _history.Dequeue();
		
		return input;
	}

	private static void HandleArrow(ConsoleKey key)
	{
		ConsoleKey[] historyNavigation = [ ConsoleKey.UpArrow, ConsoleKey.DownArrow ];

		if (historyNavigation.Contains(key) && _history.Count > 0)
		{
			int dir = key == ConsoleKey.UpArrow ? 1 : -1;
			int min = key == ConsoleKey.UpArrow ? 1 : 0;
			
			_historyCursor = Math.Clamp(_historyCursor + dir, min, _history.Count);
			HandleBackspace(_stringBuilder!.Length);

			if (_historyCursor > 0)
			{
				_history.ElementAt(new Index(_historyCursor, true)).ToList().ForEach(HandleAlphanumeric);
			}
		}
	}

	private static bool IsArrow(ConsoleKey key) =>
		_arrowKeys.Contains(key);

	private static bool IsAlphanumeric(ConsoleKey key) =>
		!_controlKeys.Contains(key);

	private static bool IsEnter(ConsoleKey key) =>
		key == ConsoleKey.Enter;

	private static bool IsBackspace(ConsoleKey key) =>
		key == ConsoleKey.Backspace;

	private static bool IsTab(ConsoleKey key) =>
		key == ConsoleKey.Tab;

	private static bool CanBackspace() =>
		_stringBuilder!.Length > 0;

	private static void HandleAlphanumeric(string text)
	{
		Output.Write(text);
		_stringBuilder!.Append(text);
	}

	private static void HandleAlphanumeric(char character)
	{
		HandleAlphanumeric(character.ToString());
	}

	private static void HandleEnter()
	{
		Output.WriteLine();
	}

	private static void HandleBackspace(int length = 1)
	{
		RemoveLastKeys(length);

		length.Times(() => MoveConsoleCursor(-1, 0));
		length.Times(() => Output.Write(" "));
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