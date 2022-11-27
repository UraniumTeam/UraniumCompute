using UraniumCompute.Compiler.Decompiling;

namespace UraniumCompute.Compiler.Syntax;

internal class CallExpressionSyntax : ExpressionSyntax
{
    internal ExpressionSyntax[] Arguments { get; }
    public override TypeSymbol ExpressionType { get; }
    internal string FunctionName { get; }

    public CallExpressionSyntax(string functionName, IEnumerable<ExpressionSyntax> arguments, TypeSymbol returnType)
    {
        Arguments = arguments.ToArray();
        FunctionName = functionName;
        ExpressionType = returnType;
    }
    
    public override string ToString()
    {
        return $"{FunctionName}{string.Join<ExpressionSyntax>(", ", Arguments)}";
    }
}
