namespace UraniumCompute.Compiler.Syntax;

internal sealed class ElseClauseSyntax : SyntaxNode
{
    public BlockStatementSyntax Block { get; }

    public ElseClauseSyntax(BlockStatementSyntax block)
    {
        Block = block;
    }

    public override string ToString()
    {
        return $"else {Block}";
    }
}
