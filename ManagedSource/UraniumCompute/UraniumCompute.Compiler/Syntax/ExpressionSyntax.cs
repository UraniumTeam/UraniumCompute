namespace UraniumCompute.Compiler.Syntax;

public abstract class ExpressionSyntax : SyntaxNode
{
    internal static readonly HashSet<string> Expressions = new()
    {
        "add", "sub", "mul", "div", "div.un", "rem", "rem.un",
        "ceq", "cgt", "cgt.un", "clt", "clt.un",
        "and", "or", "xor", "shl", "shr", "shr.un",
        "not", "neg"
    };
}