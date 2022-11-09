namespace UraniumCompute.Compiler.Syntax;

public class VariableExpressionSyntax : ExpressionSyntax
{
    internal string Name { get; }
    
    public VariableExpressionSyntax(string name)
    {
        Name = name;
    }

    public override string ToString()
    {
        return Name;
    }
}
