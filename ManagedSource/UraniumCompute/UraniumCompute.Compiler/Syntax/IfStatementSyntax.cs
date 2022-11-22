namespace UraniumCompute.Compiler.Syntax;

internal sealed class IfStatementSyntax : StatementSyntax
{
    public ExpressionSyntax Condition { get; }
    public BlockStatementSyntax ThenBlock { get; }
    public BlockStatementSyntax? ElseBlock { get; }

    public bool HasElseBlock => ElseBlock?.Statements.Any() ?? false;

    public IfStatementSyntax(ExpressionSyntax condition, BlockStatementSyntax thenBlock, BlockStatementSyntax? elseBlock)
    {
        Condition = condition;
        ThenBlock = thenBlock;
        ElseBlock = elseBlock;
    }

    public override string ToString()
    {
        return HasElseBlock
            ? $"if ({Condition}) {ThenBlock} else {ElseBlock}"
            : $"if ({Condition}) {ThenBlock}";
    }
}
