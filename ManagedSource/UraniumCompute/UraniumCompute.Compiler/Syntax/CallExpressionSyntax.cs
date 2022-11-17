namespace UraniumCompute.Compiler.Syntax;

internal class CallExpressionSyntax : ExpressionSyntax
{
    internal ExpressionSyntax[] Arguments { get; }
    
    internal string FunctionName { get; }

    public CallExpressionSyntax(string functionName, IEnumerable<ExpressionSyntax> arguments)
    {
        Arguments = arguments.ToArray();
        FunctionName = functionName;
    }
    
    public override string ToString()
    {
        return $"{FunctionName}{string.Join<ExpressionSyntax>(", ", Arguments)}";
    }
}
