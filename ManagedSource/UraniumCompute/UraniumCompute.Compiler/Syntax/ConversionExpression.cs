using UraniumCompute.Compiler.Decompiling;

namespace UraniumCompute.Compiler.Syntax;

internal sealed class ConversionExpression : ExpressionSyntax
{
    public override TypeSymbol ExpressionType { get; }
    public ExpressionSyntax ConvertedExpression { get; }

    public ConversionExpression(ExpressionSyntax convertedExpression, TypeSymbol type)
    {
        ConvertedExpression = convertedExpression;
        ExpressionType = type;
    }

    public override string ToString()
    {
        return $"({ExpressionType})({ConvertedExpression})";
    }
}
