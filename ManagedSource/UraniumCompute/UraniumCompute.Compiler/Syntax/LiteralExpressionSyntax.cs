using System.Globalization;
using UraniumCompute.Compiler.Decompiling;

namespace UraniumCompute.Compiler.Syntax;

internal class LiteralExpressionSyntax : ExpressionSyntax
{
    internal object Value { get; }
    public override TypeSymbol ExpressionType { get; }

    internal LiteralExpressionSyntax(object value)
    {
        Value = value;
        ExpressionType = TypeResolver.CreateType(value.GetType(), _ => { });
    }

    public override string ToString()
    {
        return string.Format(CultureInfo.InvariantCulture, "{0}", Value).ToLower();
    }
}
