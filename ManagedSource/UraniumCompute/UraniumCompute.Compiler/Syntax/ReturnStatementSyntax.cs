namespace UraniumCompute.Compiler.Syntax;

internal class ReturnStatementSyntax : ExpressionStatementSyntax
{
    internal ReturnStatementSyntax(ExpressionSyntax expression) : base(expression)
    {
    }

    public override string ToString()
    {
        return $"return {Expression};";
    }
}
