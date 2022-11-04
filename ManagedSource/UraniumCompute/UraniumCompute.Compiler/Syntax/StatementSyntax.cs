namespace UraniumCompute.Compiler.Syntax;

public abstract class StatementSyntax : SyntaxNode
{
    internal static readonly HashSet<string> Statements = new()
    {
        "ldc", "ldloc",
        "stloc",
        "ret"
    };
}