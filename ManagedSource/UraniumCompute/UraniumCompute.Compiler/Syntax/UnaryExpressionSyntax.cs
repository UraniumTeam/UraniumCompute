using UraniumCompute.Compiler.Decompiling;

namespace UraniumCompute.Compiler.Syntax;

internal class UnaryExpressionSyntax : ExpressionSyntax
{
    internal UnaryOperationKind Kind { get; }
    internal ExpressionSyntax Expression { get; }
    public override TypeSymbol ExpressionType => Expression.ExpressionType;

    internal UnaryExpressionSyntax(UnaryOperationKind kind, ExpressionSyntax expression)
    {
        Kind = kind;
        Expression = expression;
    }

    internal static string GetOperationString(UnaryOperationKind kind)
    {
        return kind switch
        {
            UnaryOperationKind.BitwiseNot => "~",
            UnaryOperationKind.Neg => "-",
            UnaryOperationKind.LogicalNot => "!",
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
        };
    }

    public override string ToString()
    {
        return $"({GetOperationString(Kind)} {Expression})";
    }
}
