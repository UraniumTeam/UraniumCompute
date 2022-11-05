namespace UraniumCompute.Compiler.Syntax;

public class LiteralExpressionSyntax : ExpressionSyntax
{
    internal object? Value { get; }

    public LiteralExpressionSyntax(object? value)
    {
        Value = value;
    }

    public override string ToString()
    {
        return Value?.ToString() ?? "null";
    }
}
