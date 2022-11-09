namespace UraniumCompute.Compiler.Syntax;

public class AssignmentExpressionSyntax : ExpressionStatementSyntax
{
    internal int VarNumber { get; }

    public AssignmentExpressionSyntax(int varNumber, ExpressionSyntax expression) : base(expression)
    {
        VarNumber = varNumber;
    }

    public override string ToString()
    {
        return $"V_{VarNumber} = {Expression};";
    }
}
