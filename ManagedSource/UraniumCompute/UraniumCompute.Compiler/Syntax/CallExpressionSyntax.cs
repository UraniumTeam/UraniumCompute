namespace UraniumCompute.Compiler.Syntax;

internal class CallExpressionSyntax : ExpressionSyntax
{
    internal ExpressionSyntax Variable { get; }
    
    internal ExpressionSyntax Method { get; }

    public CallExpressionSyntax(ExpressionSyntax method, ExpressionSyntax variable)
    {
        Variable = variable;
        Method = method;
    }
    
    public override string ToString()
    {
        return $"{Variable}{Method}";
    }
}
