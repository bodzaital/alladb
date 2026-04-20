namespace Adelaida;

[AttributeUsage(AttributeTargets.Method)]
public class EvaluatorAttribute(string name) : Attribute
{
    public string Name { get; set; } = name;
}