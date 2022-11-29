namespace UraniumCompute.Compiler.Syntax;

internal class ReturnStatementSyntax : ControlFlowStatement
{
    public ExpressionSyntax? Expression { get; }

    internal ReturnStatementSyntax(ExpressionSyntax? expression = null)
    {
        Expression = expression;
    }

    public override string ToString()
    {
        return $"return {Expression};";
    }
}
