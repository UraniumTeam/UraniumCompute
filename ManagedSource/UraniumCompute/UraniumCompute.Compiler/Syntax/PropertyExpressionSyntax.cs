namespace UraniumCompute.Compiler.Syntax;

internal class PropertyExpressionSyntax : ExpressionSyntax
{
    internal ExpressionSyntax Instance { get; }
    internal string PropertyName { get; }

    public PropertyExpressionSyntax(ExpressionSyntax instance, string propertyName)
    {
        Instance = instance;
        PropertyName = propertyName;
    }

    public override string ToString()
    {
        return $"{Instance}.{PropertyName}";
    }
}
