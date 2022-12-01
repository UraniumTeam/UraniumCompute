using UraniumCompute.Compiler.Decompiling;

namespace UraniumCompute.Compiler.Syntax;

internal class CallExpressionSyntax : ExpressionSyntax
{
    public override TypeSymbol ExpressionType => CalledFunction.ReturnType;
    public FunctionSymbol CalledFunction { get; }
    public List<ExpressionSyntax> Arguments { get; }

    public CallExpressionSyntax(FunctionSymbol calledFunction, IEnumerable<ExpressionSyntax> arguments)
    {
        CalledFunction = calledFunction;
        Arguments = arguments.ToList();
    }

    public override string ToString()
    {
        return $"{CalledFunction.FullName}({string.Join(", ", Arguments)})";
    }
}
