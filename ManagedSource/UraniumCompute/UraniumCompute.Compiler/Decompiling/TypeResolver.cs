using System.Numerics;
using Mono.Cecil;
using Mono.Collections.Generic;
using UraniumCompute.Common;
using UraniumCompute.Compiler.InterimStructs;

namespace UraniumCompute.Compiler.Decompiling;

internal static class TypeResolver
{
    private static string ConvertPrimitiveType(TypeReference tr)
    {
        // TODO: handle errors without exceptions (use Diagnostics)
        return tr.Name switch
        {
            "Void" => "void",
            nameof(Byte) or nameof(SByte) =>
                throw new InvalidOperationException("8-bit ints are not supported by GPU"),
            nameof(Int16) or nameof(UInt16) =>
                throw new InvalidOperationException("16-bit ints are not supported by GPU"),
            nameof(Int64) or nameof(UInt64) =>
                throw new InvalidOperationException("64-bit ints are not supported by GPU"),
            nameof(Int32) => "int",
            nameof(UInt32) => "uint",
            nameof(Single) => "float",
            nameof(Double) => "double",
            nameof(Boolean) => "bool",
            nameof(Vector2) => "float2",
            nameof(Vector3) => "float3",
            nameof(Vector4) => "float4",
            _ => throw new Exception($"Unknown type: {tr.Name}")
        };
    }

    internal static string ConvertType(TypeReference tr)
    {
        if (tr is GenericInstanceType instance)
        {
            if (instance.Namespace == "System")
            {
                return instance.Name switch
                {
                    "Span`1" => $"RWStructuredBuffer<{ConvertPrimitiveType(instance.GenericArguments[0])}>",
                    _ => throw new Exception($"Unknown generic type: {instance.Name}")
                };
            }

            throw new Exception($"Unknown namespace: {instance.Namespace}");
        }

        var customAttributes = tr.Resolve().CustomAttributes;
        var attribute = customAttributes
            .FirstOrDefault(x => x.AttributeType.Name == nameof(DeviceTypeAttribute))?
            .ConstructorArguments[0].Value;

        if (attribute is string typeName)
        {
            return typeName;
        }

        if (tr.Namespace == typeof(int).Namespace)
        {
            return ConvertPrimitiveType(tr);
        }

        throw new Exception($"Unknown type: {tr}");
    }
}
