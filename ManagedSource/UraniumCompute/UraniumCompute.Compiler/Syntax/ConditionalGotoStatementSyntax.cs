namespace UraniumCompute.Compiler.Syntax;

internal class ConditionalGotoStatementSyntax : ControlFlowStatement
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
