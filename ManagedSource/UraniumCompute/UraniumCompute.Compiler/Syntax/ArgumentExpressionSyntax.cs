using UraniumCompute.Compiler.Decompiling;

namespace UraniumCompute.Compiler.Syntax;

internal class ArgumentExpressionSyntax : ExpressionSyntax
{
    internal string Name { get; }
    internal TypeSymbol ArgumentType { get; }
    
    public ArgumentExpressionSyntax(string name, TypeSymbol argumentType)
    {
        Name = name;
        ArgumentType = argumentType;
    }

    public override string ToString()
    {
        return Name;
    }
}
