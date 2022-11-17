namespace UraniumCompute.Compiler.Syntax;

internal class AssignmentStatementSyntax : StatementSyntax
{
    internal ExpressionSyntax Left { get; }
    internal ExpressionSyntax Right { get; }

    internal AssignmentStatementSyntax(ExpressionSyntax right, ExpressionSyntax left)
    {
        Left = left;
        Right = right;
    }

    public override string ToString()
    {
        return $"{Left} = {Right};";
    }
}
