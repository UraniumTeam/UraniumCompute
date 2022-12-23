using System.Numerics;
using System.Reflection;
using Mono.Cecil;
using UraniumCompute.Common;
using UraniumCompute.Common.Math;

namespace UraniumCompute.Compiler.Decompiling;

internal static class TypeResolver
{
    private static readonly Dictionary<string, TypeSymbol> typeCache = new();
    private static readonly Dictionary<Type, TypeReference> typeRefCache = new();

    internal static IReadOnlyList<Type> SupportedMatrixTypes { get; }

    internal static IReadOnlyList<Type> SupportedVectorTypes { get; }

    static TypeResolver()
    {
        var deviceTypes = typeof(Matrix2x2).Assembly
            .GetTypes()
            .Where(x => x.GetCustomAttribute<DeviceTypeAttribute>() is not null)
            .ToArray();
        SupportedMatrixTypes = deviceTypes
            .Where(x => x.Name.StartsWith("Matrix"))
            .Append(typeof(Matrix4x4))
            .ToArray();
        SupportedVectorTypes = deviceTypes
            .Where(x => x.Name.StartsWith("Vector"))
            .Append(typeof(Vector2))
            .Append(typeof(Vector3))
            .Append(typeof(Vector4))
            .ToArray();
    }

    internal static void Reset()
    {
        typeCache.Clear();
        typeRefCache.Clear();
    }

    internal static TypeSymbol CreateType<T>()
    {
        return CreateType(typeof(T), _ => { });
    }

    internal static TypeSymbol CreateType(Type type, Action<TypeReference> userTypeCallback)
    {
        if (typeRefCache.ContainsKey(type))
        {
            return typeCache[typeRefCache[type].FullName];
        }

        var a = AssemblyDefinition.ReadAssembly(type.Assembly.Location)!;
        typeRefCache[type] = a.MainModule.ImportReference(type)!;
        return CreateType(typeRefCache[type], userTypeCallback);
    }

    internal static TypeSymbol CreateType(TypeReference tr, Action<TypeReference> userTypeCallback)
    {
        if (typeCache.ContainsKey(tr.FullName))
        {
            return typeCache[tr.FullName];
        }

        return typeCache[tr.FullName] = CreateTypeImpl(tr, userTypeCallback);
    }

    private static TypeSymbol CreateTypeImpl(TypeReference tr, Action<TypeReference> typeCallback)
    {
        if (tr is GenericInstanceType instance)
        {
            if (instance.Namespace == "System")
            {
                var argument = CreateType(instance.GenericArguments[0], typeCallback);
                return instance.Name switch
                {
                    "Span`1" => new GenericBufferTypeSymbol("RWStructuredBuffer", argument),
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
            return CreatePrimitiveType(tr);
        }

        if (tr.Namespace == typeof(Vector2).Namespace)
        {
            return tr.Name switch
            {
                nameof(Vector2) => StructTypeSymbol.CreateSystemType("float2", tr),
                nameof(Vector3) => StructTypeSymbol.CreateSystemType("float3", tr),
                nameof(Vector4) => StructTypeSymbol.CreateSystemType("float4", tr),
                nameof(Matrix4x4) => StructTypeSymbol.CreateSystemType("float4x4", tr),
                _ => throw new Exception($"Unknown type: {tr.Name}")
            };
        }

        typeCallback(tr);
        return StructTypeSymbol.CreateUserType(tr, typeCallback);
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
