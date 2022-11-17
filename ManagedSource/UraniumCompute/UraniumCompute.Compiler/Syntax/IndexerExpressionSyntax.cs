namespace UraniumCompute.Compiler.Syntax;

internal class IndexerExpressionSyntax : ExpressionSyntax
{
    internal object Index { get; }

    public IndexerExpressionSyntax(ExpressionSyntax index)
    {
        switch (index)
        {
            case LiteralExpressionSyntax litExpr:
                Index = litExpr.Value!;
                break;
            case VariableExpressionSyntax varExpr:
                Index = varExpr.Index;
                break;
            case ParameterExpressionSyntax paramExpr:
                Index = paramExpr.Name;
                break;
            default:
                throw new ArgumentException("Unsupported index type");
        }
    }
    
    public override string ToString()
    {
        return $"[{Index}]";
    }
}
