namespace UraniumCompute.Compiler.Syntax;

internal class ArgumentExpressionSyntax : ExpressionSyntax
{
    internal string Name { get; }
    
    public ArgumentExpressionSyntax(string name)
    {
        Name = name;
    }

    public override string ToString()
    {
        return Name;
    }
}
