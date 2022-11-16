namespace UraniumCompute.Compiler.Syntax;

internal class ParameterExpressionSyntax: ExpressionSyntax
{
    internal string TypeP { get; }
    internal string Name { get; }
    
    public ParameterExpressionSyntax(string type, string name)
    {
        TypeP = type;
        Name = name;
    }

    public override string ToString()
    {
        return $"{TypeP} {Name}";
    }
}
