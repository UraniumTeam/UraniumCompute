namespace UraniumCompute.Compiler.Syntax;

public class ReturnStatementSyntax:ExpressionStatementSyntax
{
    public ReturnStatementSyntax(ExpressionSyntax expression) : base(expression)
    {
    }
}