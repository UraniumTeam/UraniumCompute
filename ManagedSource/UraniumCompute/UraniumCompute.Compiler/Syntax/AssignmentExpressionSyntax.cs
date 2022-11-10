namespace UraniumCompute.Compiler.Syntax;

internal class AssignmentExpressionSyntax : ExpressionStatementSyntax
{
    internal int VarNumber { get; }

    internal AssignmentExpressionSyntax(int varNumber, ExpressionSyntax expression) : base(expression)
    {
        VarNumber = varNumber;
    }

    public override string ToString()
    {
        return $"V_{VarNumber} = {Expression};";
    }
}
