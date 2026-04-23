namespace Adelaida;

[AttributeUsage(AttributeTargets.Method)]
public class EvaluatorMethodAttribute(string name) : Attribute
{
    public string Name { get; set; } = name;
}

[AttributeUsage(AttributeTargets.Method)]
public class EvaluatorDescriptionAttribute(string text) : Attribute
{
    public string Text { get; set; } = text;
}