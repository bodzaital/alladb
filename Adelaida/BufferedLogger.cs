using System.Runtime.CompilerServices;

namespace Adelaida;

public static class BufferedLogger
{
	private class LogEntry(LogLevel level, string message, string? caller = null)
	{
		public LogLevel Level { get; } = level;
		public string Message { get; } = message;
		public DateTime DateTime { get; } = DateTime.Now;
		public string? Caller { get; } = caller;
	}

	private static readonly List<LogEntry> _logBuffer = [];

	private static bool _isEnabled = false;

	public static void Enable() => _isEnabled = true;

	public static void WriteLine()
	{
		if (!_isEnabled) return;

		_logBuffer.ForEach((logEntry) =>
		{
			ConsoleColor fg = logEntry.Level switch
			{
				LogLevel.Warning => ConsoleColor.Yellow,
				LogLevel.Error => ConsoleColor.Red,
				_ => ConsoleColor.White,
			};

			string label = logEntry.Level switch
			{
				LogLevel.Warning => "[warn]",
				LogLevel.Error => "[err!]",
				_ => "[info]",
			};

			Output.Write(fg, label);
			Output.WriteLine($" {logEntry.DateTime:yyyy-MM-dd HH:mm:ss.fff} @ {logEntry.Caller}: {logEntry.Message}");
		});
	}

	private static void Log(LogLevel level, string message, string? caller = null)
	{
		if (!_isEnabled) return;

		_logBuffer.Add(new(level, message, caller));
	}

	public static void LogInformation(string message, [CallerMemberName] string? caller = null) => Log(LogLevel.Information, message, caller);

	public static void LogWarning(string message, [CallerMemberName] string? caller = null) => Log(LogLevel.Warning, message, caller);

	public static void LogError(string message, [CallerMemberName] string? caller = null) => Log(LogLevel.Error, message, caller);
}

public enum LogLevel
{
	Information,
	Warning,
	Error,
}