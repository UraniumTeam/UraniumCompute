using System.Numerics;
using UraniumCompute.Common.Math;
using UraniumCompute.Compiler.Decompiling;

namespace UraniumCompute.Compiler.Syntax;

internal class PropertyExpressionSyntax : ExpressionSyntax
{
    internal ExpressionSyntax Instance { get; }
    internal string PropertyName { get; }
    public override TypeSymbol ExpressionType { get; }

    public PropertyExpressionSyntax(ExpressionSyntax instance, string propertyName)
    {
        Instance = instance;
        PropertyName = propertyName;
        ExpressionType = ResolveType(instance.ExpressionType);
    }

    private static TypeSymbol ResolveType(TypeSymbol instanceType)
    {
        switch (instanceType.FullName)
        {
            case "float2":
            case "float3":
            case "float4":
                return TypeResolver.CreateType(typeof(float));
            case "int2":
            case "int3":
            case "int4":
                return TypeResolver.CreateType(typeof(int));
            case "uint2":
            case "uint3":
            case "uint4":
                return TypeResolver.CreateType(typeof(uint));
        }

        throw new ArgumentException($"Unsupported type: {instanceType}");
    }

    public override string ToString()
    {
        return $"{Instance}.{PropertyName}";
    }
}
