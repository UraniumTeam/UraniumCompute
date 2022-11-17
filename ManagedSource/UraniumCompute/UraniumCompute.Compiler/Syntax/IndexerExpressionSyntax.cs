namespace UraniumCompute.Compiler.Syntax;

internal class IndexerExpressionSyntax : ExpressionSyntax
{
    internal ExpressionSyntax Index { get; }
    internal ExpressionSyntax IndexedExpression { get; }

    public IndexerExpressionSyntax(ExpressionSyntax index, ExpressionSyntax indexedExpression)
    {
        Index = index;
        IndexedExpression = indexedExpression;
    }
    
    public override string ToString()
    {
        return $"{IndexedExpression}[{Index}]";
    }
}
