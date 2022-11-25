using UraniumCompute.Compiler.Decompiling;

namespace UraniumCompute.Compiler.Syntax;

internal class CallExpressionSyntax : ExpressionSyntax
{
    internal ExpressionSyntax[] Arguments { get; }
    internal TypeSymbol ReturnType { get; }
    internal string FunctionName { get; }

    public CallExpressionSyntax(string functionName, IEnumerable<ExpressionSyntax> arguments, TypeSymbol returnType)
    {
        Arguments = arguments.ToArray();
        FunctionName = functionName;
        ReturnType = returnType;
    }
    
    public override string ToString()
    {
        return $"{FunctionName}{string.Join<ExpressionSyntax>(", ", Arguments)}";
    }
}
