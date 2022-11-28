using System.Numerics;
using Mono.Cecil;
using UraniumCompute.Common.Math;
using UraniumCompute.Compiler.Decompiling;

namespace UraniumCompute.Compiler.Syntax;

internal class PropertyExpressionSyntax : ExpressionSyntax
{
    internal ExpressionSyntax Instance { get; }
    internal string PropertyName { get; }
    public override TypeSymbol ExpressionType { get; }

    public PropertyExpressionSyntax(ExpressionSyntax instance, FieldReference fieldReference)
    {
        Instance = instance;

        if (instance.ExpressionType.TryGetFieldDesc(fieldReference, out var fieldDesc))
        {
            PropertyName = fieldDesc.Name;
            ExpressionType = fieldDesc.FieldType;
        }
        else
        {
            throw new Exception($"Unknown field {fieldReference}");
        }
    }

    public override string ToString()
    {
        return $"{Instance}.{PropertyName}";
    }
}
