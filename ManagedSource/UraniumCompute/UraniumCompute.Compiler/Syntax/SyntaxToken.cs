

namespace UraniumCompute.Compiler.Syntax;

public sealed class SyntaxToken : SyntaxNode
{
    internal object Value { get; }

    internal SyntaxToken(SyntaxNode? parent, object value, SyntaxTree syntaxTree) : base(parent, syntaxTree)
    {
        Value = value;
    }
}