namespace UraniumCompute.Compiler.Syntax;

internal sealed class WhileStatementSyntax : StatementSyntax
{
    public ExpressionSyntax Condition { get; }
    public BlockStatementSyntax Block { get; }

    public WhileStatementSyntax(ExpressionSyntax condition, BlockStatementSyntax block)
    {
        Condition = condition;
        Block = block;
    }

    public static WhileStatementSyntax CreateInfinite(BlockStatementSyntax block)
    {
        return new WhileStatementSyntax(new LiteralExpressionSyntax(true), block);
    }

    public override string ToString()
    {
        return $"while ({Condition}) {Block}";
    }
}