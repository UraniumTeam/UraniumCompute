namespace UraniumCompute.Compiler.Syntax;

internal class VariableExpressionSyntax : ExpressionSyntax
{
    internal int Index { get; }
    
    public VariableExpressionSyntax(int index)
    {
        Index = index;
    }

    public override string ToString()
    {
        return $"V_{Index}";
    }
}
