namespace UraniumCompute.Compiler.Syntax;

internal class IndexerExpressionSyntax : ExpressionSyntax
{
    internal int Index { get; }

    public IndexerExpressionSyntax(LiteralExpressionSyntax index)
    {
        Index = (int)index.Value!;
    }
    
    public override string ToString()
    {
        return $"[{Index}]";
    }
}
