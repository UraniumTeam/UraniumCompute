namespace UraniumCompute.Compiler.Syntax;

public class AssignmentExpressionSyntax : ExpressionSyntax
{
    internal VariableExpressionSyntax Variable { get; }
    internal ExpressionSyntax Expression { get; }
}