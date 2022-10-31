namespace UraniumCompute.Compiler.Syntax;

public class ExpressionStatementSyntax:StatementSyntax
{
    public ExpressionStatementSyntax(ExpressionSyntax expression)
    {
        Expression = expression;
    }

    internal ExpressionSyntax Expression { get; }
    
}