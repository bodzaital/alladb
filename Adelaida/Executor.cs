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
        public required string ConnectionString { get; init; }
    }

    private Alla? _db;

    private Collection? _collection;

    private bool _isLooping = true;
    
    protected override int Execute(CommandContext context, ReplSettings settings, CancellationToken cancellationToken)
    {
        _db = new Alla(AllaOptions.FromConnectionString(settings.ConnectionString));
        Console.WriteLine("Adelaida CLI, connection established.");

        do
        {
            string collectionName = _collection?.Name ?? "no collection";
            Console.Write($"({collectionName}) > ");
            string input = Console.ReadLine()!;
            Parse(input);
        } while (_isLooping);

        _db!.Persist();
        Console.WriteLine("Bye!");
        return 0;
    }

    private void Parse(string input)
    {
        string[] split = input.Split(' ');

        if (split[0] == "exit")
        {
            _isLooping = false;
        }
        else if (split[0] == "get-collection")
        {
            _collection = _db!.GetCollection(split[1]);
        }
        else if (split[0] == "drop-collection")
        {
            _db!.DropCollection(split[1]);

            Console.WriteLine($"Dropped collection {split[1]}");
        }
        else if (split[0] == "add")
        {
            Dictionary<string, object?> fields = split[1..].ToList().Select((x) => x.Split('=')).ToDictionary((key) => key[0], (val) => (object?)val[1]);
            _collection!.Add(fields);
            Console.WriteLine($"Added new document:\n{JsonSerializer.Serialize(fields)}");
        }
    }
}