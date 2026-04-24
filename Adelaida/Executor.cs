using System.ComponentModel;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Spectre.Console.Cli;

namespace Adelaida;

public class Executor : Command<Executor.ReplSettings>
{
    public class ReplSettings : CommandSettings
    {
        [CommandOption("-c|--connection")]
        [Description("Connection string for the database")]
        [DefaultValue("Data Source = .")]
        public required string ConnectionString { get; init; }
    }
    
    protected override int Execute(CommandContext context, ReplSettings settings, CancellationToken cancellationToken)
    {
        Evaluator evaluator = new(settings.ConnectionString);
        Console.WriteLine("Adelaida CLI, REPL up and running.");

        do
        {
            string collectionName = evaluator.Collection?.Name ?? "no collection";
            string documentId = evaluator.Document?.Id is not null
                ? $" editing {evaluator.Document.Id}"
                : "";
            string handle = $"{collectionName}{documentId}";

            Console.Write($"({handle}) > ");

            Input.Setup([.. evaluator.Evaluators()]);
            string userInput = Input.ReadLine(evaluator.History);

            if (userInput.Trim() == string.Empty) continue;

            Regex r = new("(\".*?\"|\\S+)");
            MatchCollection ms = r.Matches(userInput);
            string[] input = [.. ms.Select((x) => x.Value).Select((x) => x.Trim('"'))];
            string cmd = input[0];
            string[] args = input[1..];

            evaluator.PushHistory(userInput);

            evaluator.Evaluate(cmd, args);
        } while (evaluator.IsLooping);

        Console.WriteLine("Bye!");

        return 0;
    }
}