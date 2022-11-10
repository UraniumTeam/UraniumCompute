namespace UraniumCompute.Compiler.Syntax;

internal class LiteralExpressionSyntax : ExpressionSyntax
{
    internal object? Value { get; }

    internal LiteralExpressionSyntax(object? value)
    {
        Value = value;
    }

    public override string ToString()
    {
        return Value?.ToString() ?? "null";
    }
}
