namespace UraniumCompute.Compiler.Syntax;

internal sealed class IfStatementSyntax : StatementSyntax
{
    public ExpressionSyntax Condition { get; }
    public BlockStatementSyntax ThenBlock { get; }
    public BlockStatementSyntax? ElseBlock { get; }

    public IfStatementSyntax(ExpressionSyntax condition, BlockStatementSyntax thenBlock, BlockStatementSyntax? elseBlock)
    {
        Condition = condition;
        ThenBlock = thenBlock;
        ElseBlock = elseBlock;
    }

    public override string ToString()
    {
        return ElseBlock is null
            ? $"if ({Condition}) {ThenBlock}"
            : $"if ({Condition}) {ThenBlock} else {ElseBlock}";
    }
}
