namespace UraniumCompute.Compiler.Syntax;

internal sealed class IfStatementSyntax : StatementSyntax
{
    public ExpressionSyntax Condition { get; }
    public BlockStatementSyntax ThenBlock { get; }
    public ElseClauseSyntax? ElseClause { get; }

    public IfStatementSyntax(ExpressionSyntax condition, BlockStatementSyntax thenBlock, ElseClauseSyntax? elseClause)
    {
        Condition = condition;
        ThenBlock = thenBlock;
        ElseClause = elseClause;
    }

    public override string ToString()
    {
        return $"if ({Condition}) {ThenBlock} {ElseClause}";
    }
}