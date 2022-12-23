namespace UraniumCompute.Compiler.Syntax;

internal class ExpressionStatementSyntax : StatementSyntax
{
    public ExpressionSyntax Expression { get; }

    public ExpressionStatementSyntax(ExpressionSyntax expression)
    {
        Expression = expression;
    }

    public override string ToString()
    {
        return Expression.ToString()!;
    }
}
