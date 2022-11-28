using System.Numerics;
using Mono.Cecil;
using UraniumCompute.Common;

namespace UraniumCompute.Compiler.Decompiling;

internal static class TypeResolver
{
    private static readonly Dictionary<TypeReference, TypeSymbol> typeCache = new();
    private static readonly Dictionary<Type, TypeReference> typeRefCache = new();

    internal static TypeSymbol CreateType<T>()
    {
        return CreateType(typeof(T), _ => { });
    }

    internal static TypeSymbol CreateType(Type type, Action<TypeReference> userTypeCallback)
    {
        if (typeRefCache.ContainsKey(type))
        {
            return typeCache[typeRefCache[type]];
        }

        var a = AssemblyDefinition.ReadAssembly(type.Assembly.Location)!;
        typeRefCache[type] = a.MainModule.ImportReference(type)!;
        return CreateType(typeRefCache[type], userTypeCallback);
    }

    internal static TypeSymbol CreateType(TypeReference tr, Action<TypeReference> userTypeCallback)
    {
        if (typeCache.ContainsKey(tr))
        {
            return typeCache[tr];
        }

        return typeCache[tr] = CreateTypeImpl(tr, userTypeCallback);
    }

    private static TypeSymbol CreateTypeImpl(TypeReference tr, Action<TypeReference> userTypeCallback)
    {
        if (tr is GenericInstanceType instance)
        {
            if (instance.Namespace == "System")
            {
                var argument = CreateType(instance.GenericArguments[0], userTypeCallback);
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
            return StructTypeSymbol.CreateSystemType(typeName, tr);
        }

        if (tr.Namespace == typeof(int).Namespace)
        {
            return tr.Name switch
            {
                nameof(Vector2) => StructTypeSymbol.CreateSystemType("float2", tr),
                nameof(Vector3) => StructTypeSymbol.CreateSystemType("float3", tr),
                nameof(Vector4) => StructTypeSymbol.CreateSystemType("float4", tr),
                _ => CreatePrimitiveType(tr)
            };
        }

        userTypeCallback(tr);
        return StructTypeSymbol.CreateUserType(tr, userTypeCallback);
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
            _ => throw new Exception($"Unknown type: {tr.Name}")
        };

        return new PrimitiveTypeSymbol(name);
    }
}
