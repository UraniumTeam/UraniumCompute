namespace UraniumCompute.Compiler.Syntax;

internal class GotoStatementSyntax : ControlFlowStatement
{
    public int Offset { get; }

    public GotoStatementSyntax(int offset)
    {
        Offset = offset;
    }

    public override string ToString()
    {
        return $"goto {Offset};";
    }
}
