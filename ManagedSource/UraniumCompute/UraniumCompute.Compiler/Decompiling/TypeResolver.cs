using System.Numerics;
using Mono.Cecil;
using UraniumCompute.Common;

namespace UraniumCompute.Compiler.Decompiling;

internal static class TypeResolver
{
    internal static TypeSymbol CreateType<T>()
    {
        return CreateType(typeof(T));
    }
    
    internal static TypeSymbol CreateType(Type type)
    {
        var a = AssemblyDefinition.ReadAssembly(type.Assembly.Location)!;
        var tr = a.MainModule.ImportReference(type)!;
        return CreateType(tr);
    }
    
    internal static TypeSymbol CreateType(TypeReference tr)
    {
        if (tr is GenericInstanceType instance)
        {
            if (instance.Namespace == "System")
            {
                var argument = CreateType(instance.GenericArguments[0]);
                return instance.Name switch
                {
                    "Span`1" => new GenericTypeSymbol("RWStructuredBuffer", argument),
                    _ => throw new ArgumentException($"Unknown generic type: {instance.Name}")
                };
            }

            throw new ArgumentException($"Unknown namespace: {instance.Namespace}");
        }

        var customAttributes = tr.Resolve().CustomAttributes;
        var attribute = customAttributes
            .FirstOrDefault(x => x.AttributeType.Name == nameof(DeviceTypeAttribute))?
            .ConstructorArguments[0].Value;

        if (attribute is string typeName)
        {
            return new PrimitiveTypeSymbol(typeName);
        }

        if (tr.Namespace == typeof(int).Namespace)
        {
            return CreatePrimitiveType(tr);
        }

        throw new Exception($"Unknown type: {tr}");
    }
    
    private static PrimitiveTypeSymbol CreatePrimitiveType(TypeReference tr)
    {
        // TODO: handle errors without exceptions (use Diagnostics)
        var name = tr.Name switch
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

        return new PrimitiveTypeSymbol(name);
    }
}
