using System.Text.Json;
using System.Text.RegularExpressions;

namespace Adelaida.Evaluators;

public abstract class EvaluatorBase
{
    protected static JsonSerializerOptions PrettySerializer = new()
    {
        WriteIndented = true,
    };

    protected static bool RequiresArguments(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("This requires arguments.");
            return true;
        }

        return false;
    }

    protected static bool RequiresConfirmation(string msg = "Are you sure?")
    {
        Console.Write($"{msg} (y,N) > ");
        ConsoleKeyInfo answer = Console.ReadKey();
        
        Console.WriteLine();

        return answer.Key != ConsoleKey.Y;
    }
    
    protected static T? ParseFieldValueWithType<T>(string input)
    {
        if (input == "(null)") return default;

        Regex r = new(@"(?:\((?<type>\w+)\))?(?<value>.+)");
        Match m = r.Match(input);

        Group type = m.Groups["type"];
        Group value = m.Groups["value"];

        Type valueType = typeof(string);

        if (type.Success)
        {
            valueType = type.Value switch
            {
                "int" => typeof(int),
                "bool" => typeof(bool),
                _ => valueType,
            };
        }

        if (!value.Success)
        {
            Console.WriteLine($"Failed to parse value '{input}' with regex.");
            Console.WriteLine($"Parsed type was '{valueType}'.");
            throw new Exception("No field value.");
        }

        return (T)Convert.ChangeType(value.Value, valueType);
    }
}