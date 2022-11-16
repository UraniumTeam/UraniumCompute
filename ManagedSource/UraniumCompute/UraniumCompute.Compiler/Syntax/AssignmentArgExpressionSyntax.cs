namespace UraniumCompute.Compiler.Syntax;

internal class AssignmentArgExpressionSyntax : ExpressionStatementSyntax
{
    internal ExpressionSyntax Arg { get; }

    internal AssignmentArgExpressionSyntax(ExpressionSyntax expression, ExpressionSyntax arg) : base(expression)
    {
        Arg = arg;
    }

    public override string ToString()
    {
        return $"{Arg} = {Expression};";
    }
}
