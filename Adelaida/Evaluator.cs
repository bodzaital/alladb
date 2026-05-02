using System.Reflection;
using Adelaida.Evaluators;

namespace Adelaida;

public class Evaluator
{
    private readonly List<EvaluatorInfo> _evaluators = [];
    public bool IsLooping { get; set; } = true;

    public Evaluator(Context ctx)
    {
        IEnumerable<EvaluatorBase?> inheritedEvaluators = Assembly.GetAssembly(GetType())?.GetTypes()
            .Where((x) => x.IsClass && !x.IsAbstract && x.IsSubclassOf(typeof(EvaluatorBase)))
            .Select((x) => (EvaluatorBase?)Activator.CreateInstance(x, ctx)) ?? [];

        foreach (EvaluatorBase? inheritedEvaluator in inheritedEvaluators)
        {
            if (inheritedEvaluator is null) continue;

            _evaluators.AddRange(BuildEvaluators(inheritedEvaluator));
        }
        
        _evaluators.AddRange(BuildEvaluators(this));
    }

    private List<EvaluatorInfo> BuildEvaluators(object target) => [.. target.GetType().GetMethods()
        .Where((x) => x.GetCustomAttribute<EvaluatorMethodAttribute>() is not null)
        .Select((x) => new EvaluatorInfo(
            x.GetCustomAttribute<EvaluatorMethodAttribute>()!.Name,
            x.GetCustomAttribute<EvaluatorDescriptionAttribute>()?.Text ?? string.Empty,
            x.CreateDelegate<Action<string[]>>(target)
        ))
    ];

    public void Evaluate(string name, string[] args)
    {
        EvaluatorInfo? evaluatorInfo = _evaluators.Find((x) => x.Name == name);
        if (evaluatorInfo is null)
        {
            Output.WriteLine(ConsoleColor.Red, $"No function called {name}.");
            return;
        }

        evaluatorInfo.Action.Invoke(args);
    }

    public List<string> Evaluators() =>
        [.. _evaluators.Select((x) => x.Name)];

    [EvaluatorMethod("help")]
    public void Help(string[] args)
    {
        if (args.Length == 0)
        {
            Output.WriteLine("List of functions:");
            Output.WriteLine(string.Join(", ", Evaluators()));
            Output.WriteLine("Type \"help [function]\" for more info.");
            return;
        }

        Dictionary<string, string> autocompletions = _evaluators
			.Where((x) => x.Name.StartsWith(args.First()))
            .ToDictionary((x) => x.Name, (x) => x.Description);

        KeyValuePair<string, string> foundFunction = autocompletions.FirstOrDefault((x) => x.Key == args.First());

        bool foundSpecificFunction = autocompletions.Count == 1;
        bool foundExactInAutocompletions = !foundFunction.Equals(default(KeyValuePair<string, string>));

        if (foundSpecificFunction || foundExactInAutocompletions)
        {
            KeyValuePair<string, string> function = foundSpecificFunction
                ? autocompletions.First()
                : foundFunction;

            bool hasDescription = function.Value != string.Empty;

            string text = hasDescription
                ? $"{function.Key}: {function.Value}"
                : function.Key;

            Output.WriteLine(text);
            return;
        }
        else if (autocompletions.Count > 0)
        {
            Output.WriteLine("List of functions:");
            Output.WriteLine(string.Join(", ", autocompletions.Select((x) => x.Key)));
            return;
        }

        Output.WriteLine(ConsoleColor.Red, "No function found.");
    }

    [EvaluatorMethod("exit")]
    [EvaluatorDescription("Exits the CLI.")]
    public void Exit(string[] args)
    {
        IsLooping = false;
    }
}