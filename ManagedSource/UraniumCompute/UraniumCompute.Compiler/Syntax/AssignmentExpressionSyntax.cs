namespace UraniumCompute.Compiler.Syntax;

internal class AssignmentExpressionSyntax : ExpressionStatementSyntax
{
    internal ExpressionSyntax LeftOperand { get; }

    internal AssignmentExpressionSyntax(ExpressionSyntax rightOperand, ExpressionSyntax leftOperand) : base(rightOperand)
    {
        LeftOperand = leftOperand;
    }

    public override string ToString()
    {
        return $"{LeftOperand} = {Expression};";
    }
}
