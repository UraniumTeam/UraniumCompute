namespace UraniumCompute.Compiler.Syntax;

internal class ParameterExpressionSyntax: ExpressionSyntax
{
    internal string ParameterType { get; }
    internal string Name { get; }
    
    public ParameterExpressionSyntax(string parameterType, string name)
    {
        ParameterType = parameterType;
        Name = name;
    }

    public override string ToString()
    {
        return $"{Name}";
    }
    
    public string ToStringWithType()
    {
        return $"{ParameterType} {Name}";
    }
}
