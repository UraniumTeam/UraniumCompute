namespace UraniumCompute.Compiler.Syntax;

internal class UnaryExpressionSyntax : ExpressionSyntax
{
    internal UnaryOperationKind Kind { get; }
    internal ExpressionSyntax Expression { get; }

    internal UnaryExpressionSyntax(UnaryOperationKind kind, ExpressionSyntax expression)
    {
        Kind = kind;
        Expression = expression;
    }

    private static string GetOperation(UnaryOperationKind kind)
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
        return $"({GetOperation(Kind)} {Expression})";
    }
}
