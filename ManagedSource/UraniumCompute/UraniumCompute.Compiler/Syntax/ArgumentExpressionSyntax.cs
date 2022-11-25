using UraniumCompute.Compiler.Decompiling;

namespace UraniumCompute.Compiler.Syntax;

internal class ArgumentExpressionSyntax : ExpressionSyntax
{
    internal string Name { get; }
    public override TypeSymbol ExpressionType { get; }
    
    public ArgumentExpressionSyntax(string name, TypeSymbol argumentType)
    {
        Name = name;
        ExpressionType = argumentType;
    }

    public override string ToString()
    {
        return Name;
    }
}
