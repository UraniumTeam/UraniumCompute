namespace UraniumCompute.Compiler.Syntax;

internal class AssignmentVarExpressionSyntax : ExpressionStatementSyntax
{
    internal int VarNumber { get; }

    internal AssignmentVarExpressionSyntax(int varNumber, ExpressionSyntax expression) : base(expression)
    {
        VarNumber = varNumber;
    }

    public override string ToString()
    {
        return $"V_{VarNumber} = {Expression};";
    }
}
