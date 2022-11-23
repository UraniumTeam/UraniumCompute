namespace UraniumCompute.Compiler.Syntax;

internal class ParameterDeclarationSyntax : SyntaxNode
{
    internal string ParameterType { get; }
    internal string Name { get; }
    internal int BindingIndex { get; }

    public ParameterDeclarationSyntax(string parameterType, string name, int bindingIndex)
    {
        ParameterType = parameterType;
        Name = name;
        BindingIndex = bindingIndex;
    }

    public override string ToString()
    {
        return $"{Name}";
    }

    public string ToStringWithType()
    {
        return $"{ParameterType} {Name}";
    }
}
