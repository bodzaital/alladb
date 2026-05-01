using System.ComponentModel;
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

        [CommandOption("-v|--verbose")]
        [Description("Enabled verbose logging that is written once the REPL normally exits.")]
        [DefaultValue(false)]
        public required bool IsVerbose { get; init; }
    }
    
    protected override int Execute(CommandContext context, ReplSettings settings, CancellationToken cancellationToken)
    {
        if (settings.IsVerbose)
        {
            Output.WriteLine(ConsoleColor.Yellow, "Running in buffered verbose mode.");
            BufferedLogger.Enable();
        }

        Context ctx = new(settings.ConnectionString);
        Evaluator evaluator = new(ctx);
        Output.WriteLine("Adelaida CLI, REPL up and running.");

        do
        {
            Output.Write($"{ctx.GetHandle()} > ");

            Input.Setup([.. evaluator.Evaluators()]);
            string userInput = Input.ReadLine();

            if (userInput.Trim() == string.Empty) continue;

            if (userInput == "history")
            {
                Input.WriteHistory();
                continue;
            }

            Regex r = new("(\".*?\"|\\S+)");
            MatchCollection ms = r.Matches(userInput);
            string[] input = [.. ms.Select((x) => x.Value).Select((x) => x.Trim('"'))];
            string cmd = input[0];
            string[] args = input[1..];

            evaluator.Evaluate(cmd, args);
        } while (evaluator.IsLooping);

        Output.WriteLine("Bye!");

        if (settings.IsVerbose)
        {
            BufferedLogger.WriteLine();
        }

        return 0;
    }
}