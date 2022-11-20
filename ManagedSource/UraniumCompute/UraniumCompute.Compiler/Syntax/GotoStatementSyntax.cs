namespace UraniumCompute.Compiler.Syntax;

internal class ConditionalGotoStatementSyntax : StatementSyntax
{
    public ExpressionSyntax Condition { get; }
    public int Offset { get; }

    public ConditionalGotoStatementSyntax(ExpressionSyntax condition, int offset)
    {
        Condition = condition;
        Offset = offset;
    }

    public override string ToString()
    {
        return $"if ({Condition}) {{ goto {Offset}; }}";
    }
}

internal class GotoStatementSyntax : StatementSyntax
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
