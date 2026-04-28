namespace Adelaida;

public static class Output
{
    public static void Write(object? value = null) => Write(null, value);

    public static void Write(ConsoleColor? foreground, object? value = null)
    {
        ConsoleColor defaultForeground = Console.ForegroundColor;
        if (foreground is not null) Console.ForegroundColor = (ConsoleColor)foreground;

        Console.Write(value);

        if (foreground is not null) Console.ForegroundColor = defaultForeground;
    }

    public static void WriteLine(object? value = null) => WriteLine(null, value);

    public static void WriteLine(ConsoleColor? foreground, object? value = null)
    {
        ConsoleColor defaultForeground = Console.ForegroundColor;
        if (foreground is not null) Console.ForegroundColor = (ConsoleColor)foreground;

        Console.WriteLine(value);

        if (foreground is not null) Console.ForegroundColor = defaultForeground;
    }
}