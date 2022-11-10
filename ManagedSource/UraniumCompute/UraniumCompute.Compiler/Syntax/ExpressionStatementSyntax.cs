namespace UraniumCompute.Compiler.Syntax;

internal abstract class ExpressionStatementSyntax : StatementSyntax
{
    internal ExpressionSyntax Expression { get; }

    protected ExpressionStatementSyntax(ExpressionSyntax expression)
    {
        Expression = expression;
    }
}
