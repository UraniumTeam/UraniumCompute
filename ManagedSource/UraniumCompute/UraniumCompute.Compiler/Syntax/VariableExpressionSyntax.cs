using UraniumCompute.Compiler.Decompiling;

namespace UraniumCompute.Compiler.Syntax;

internal class VariableExpressionSyntax : ExpressionSyntax
{
    internal int Index { get; }
    public override TypeSymbol ExpressionType { get; }

    public VariableExpressionSyntax(int index, TypeSymbol expressionType)
    {
        Index = index;
        ExpressionType = expressionType;
    }

    public override string ToString()
    {
        return $"V_{Index}";
    }
}
