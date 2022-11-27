namespace UraniumCompute.Compiler.Syntax;

internal sealed class LabelStatementSyntax : StatementSyntax
{
    public int Offset { get; }

    public LabelStatementSyntax(int offset)
    {
        Offset = offset;
    }

    public override int GetHashCode()
    {
        return Offset;
    }

    public override string ToString()
    {
        return $"Label_{Offset}:";
    }
}
