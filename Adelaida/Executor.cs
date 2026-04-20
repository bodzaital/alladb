using System.ComponentModel;
using System.Text.Json;
using AllaDb;
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

            string[] input = Console.ReadLine()!.Split(' ');
            string cmd = input[0];
            string[] args = input[1..];

            evaluator.Evaluate(cmd, args);
        } while (evaluator.IsLooping);

        evaluator.Db.Persist();
        Console.WriteLine("Bye!");

        return 0;
    }
}