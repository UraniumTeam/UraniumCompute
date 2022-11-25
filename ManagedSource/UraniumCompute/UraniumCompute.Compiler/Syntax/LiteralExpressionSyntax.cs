using UraniumCompute.Compiler.Decompiling;

namespace UraniumCompute.Compiler.Syntax;

internal class LiteralExpressionSyntax : ExpressionSyntax
{
    internal object Value { get; }
    internal TypeSymbol LiteralType { get; }

    internal LiteralExpressionSyntax(object value)
    {
        Value = value;
        LiteralType = TypeResolver.CreateType(value.GetType());
    }

    public override string ToString()
    {
        return Value.ToString()!.ToLower();
    }
}
