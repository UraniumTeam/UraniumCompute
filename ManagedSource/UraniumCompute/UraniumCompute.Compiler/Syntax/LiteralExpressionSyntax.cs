using UraniumCompute.Compiler.Decompiling;

namespace UraniumCompute.Compiler.Syntax;

internal class LiteralExpressionSyntax : ExpressionSyntax
{
    internal object Value { get; }
    public override TypeSymbol ExpressionType { get; }

    internal LiteralExpressionSyntax(object value)
    {
        Value = value;
        ExpressionType = TypeResolver.CreateType(value.GetType());
    }

    public override string ToString()
    {
        return Value.ToString()!.ToLower();
    }
}
