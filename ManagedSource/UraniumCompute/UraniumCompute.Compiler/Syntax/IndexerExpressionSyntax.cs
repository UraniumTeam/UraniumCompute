using UraniumCompute.Compiler.Decompiling;

namespace UraniumCompute.Compiler.Syntax;

internal class IndexerExpressionSyntax : ExpressionSyntax
{
    internal ExpressionSyntax Index { get; }
    internal ExpressionSyntax IndexedExpression { get; }
    public override TypeSymbol ExpressionType { get; }

    public IndexerExpressionSyntax(ExpressionSyntax index, ExpressionSyntax indexedExpression)
    {
        Index = index;
        IndexedExpression = indexedExpression;
        if (IndexedExpression.ExpressionType is GenericBufferTypeSymbol type)
        {
            ExpressionType = type.Argument;
        }
        else
        {
            throw new ArgumentException($"Invalid indexed type: {IndexedExpression.ExpressionType}");
        }
    }

    public override string ToString()
    {
        return $"{IndexedExpression}[{Index}]";
    }
}
