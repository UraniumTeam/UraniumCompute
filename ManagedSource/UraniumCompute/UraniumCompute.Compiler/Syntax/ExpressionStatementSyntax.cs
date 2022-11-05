namespace UraniumCompute.Compiler.Syntax;

public class ExpressionStatementSyntax : StatementSyntax
{
    internal ExpressionSyntax Expression { get; }

    public ExpressionStatementSyntax(ExpressionSyntax expression)
    {
        Expression = expression;
    }
}
