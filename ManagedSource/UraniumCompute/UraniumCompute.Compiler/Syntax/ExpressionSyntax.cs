using UraniumCompute.Compiler.Decompiling;

namespace UraniumCompute.Compiler.Syntax;

internal abstract class ExpressionSyntax : SyntaxNode
{
    public abstract TypeSymbol ExpressionType { get; }
}
