using System.Text.Json;
using System.Text.RegularExpressions;

namespace Adelaida.Evaluators;

public abstract class EvaluatorBase
{
    protected readonly static JsonSerializerOptions PrettySerializer = new()
    {
        WriteIndented = true,
    };

    protected static bool RequiresArguments(string[] args, Dictionary<string, string> requiredArgs)
    {
        if (args.Length == 0)
        {
            Output.WriteLine(ConsoleColor.Red, "This requires arguments:");
            requiredArgs.ToList().ForEach((x) => Output.WriteLine(ConsoleColor.Red, $"  [{x.Key}]: {x.Value}"));
            return true;
        }

        return false;
    }

    protected static bool RequiresConfirmation(string msg = "Are you sure?")
    {
        Output.Write(ConsoleColor.Yellow, $"{msg} (y,N) > ");
        ConsoleKeyInfo answer = Console.ReadKey();
        
        Output.WriteLine();

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
            Output.WriteLine(ConsoleColor.Red, $"Failed to parse value '{input}' with regex.");
            Output.WriteLine(ConsoleColor.Red, $"Parsed type was '{valueType}'.");
            throw new Exception("No field value.");
        }

        return (T)Convert.ChangeType(value.Value, valueType);
    }
}