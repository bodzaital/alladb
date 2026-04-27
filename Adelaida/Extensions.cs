namespace Adelaida;

public static class Extensions
{
	public static void Times(this int count, Action<int> action)
	{
		for (int i = 1; i < count + 1; i++) action(i);
	}

	public static void Times(this int count, Action action)
	{
		for (int i = 1; i < count + 1; i++) action();
	}
}