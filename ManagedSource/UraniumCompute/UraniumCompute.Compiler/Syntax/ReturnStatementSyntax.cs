namespace UraniumCompute.Compiler.Syntax;

internal class ReturnStatementSyntax : StatementSyntax
{
    public ExpressionSyntax Expression { get; }

    internal ReturnStatementSyntax(ExpressionSyntax expression)
    {
        Expression = expression;
    }

    public override string ToString()
    {
        return $"return {Expression};";
    }
}
