namespace UraniumCompute.Compiler.Syntax;

public class ExpressionStatementSyntax : StatementSyntax
{
    internal static readonly HashSet<string> ExpressionStatements = new()
    {
        "ldc", "ldloc",
        "stloc"
    };

    internal ExpressionSyntax Expression { get; }

    public ExpressionStatementSyntax(ExpressionSyntax expression)
    {
        Expression = expression;
    }
}