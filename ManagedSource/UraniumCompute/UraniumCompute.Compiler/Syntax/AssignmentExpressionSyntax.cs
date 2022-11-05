namespace UraniumCompute.Compiler.Syntax;

public class AssignmentExpressionSyntax : ExpressionSyntax
{
    internal VariableExpressionSyntax Variable { get; }
    internal ExpressionSyntax Expression { get; }

    public AssignmentExpressionSyntax(VariableExpressionSyntax variable, ExpressionSyntax expression)
    {
        Variable = variable;
        Expression = expression;
    }
}
