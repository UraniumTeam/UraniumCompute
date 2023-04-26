using System.Numerics;
using Mono.Cecil;

namespace UraniumCompute.Compiler.Decompiling;

internal static class FunctionResolver
{
    public static FunctionSymbol Resolve(MethodReference methodReference, Action<MethodReference> userFunctionCallback,
        Action<TypeReference> userTypeCallback)
    {
        if (methodReference.DeclaringType.Namespace == typeof(MathF).Namespace
            || methodReference.DeclaringType.Namespace == typeof(Matrix4x4).Namespace
            || IsIntrinsicType(methodReference.DeclaringType))
        {
            return IntrinsicFunctionSymbol.Resolve(
                $"{methodReference.DeclaringType.Name}.{methodReference.Name}",
                methodReference.Parameters.Count);
        }

        userFunctionCallback(methodReference);
        var returnType = TypeResolver.CreateType(methodReference.ReturnType, userTypeCallback);
        var arguments = GetArguments(methodReference, userTypeCallback);
        return new UserFunctionSymbol(methodReference.Name, returnType, arguments);
    }

    private static IEnumerable<TypeSymbol> GetArguments(MethodReference methodReference, Action<TypeReference> userTypeCallback)
    {
        var arguments = methodReference.Parameters
            .Select(x => x.ParameterType)
            .Select(x => TypeResolver.CreateType(x, userTypeCallback));

        if (!methodReference.HasThis)
        {
            return arguments;
        }

        var thisType = TypeResolver.CreateType(methodReference.DeclaringType, userTypeCallback);
        return Enumerable.Repeat(thisType, 1).Concat(arguments);
    }

    private static bool IsIntrinsicType(TypeReference type)
    {
        var t = TypeResolver.CreateType(type, _ => { }, true);
        return t is StructTypeSymbol { IsIntrinsicType: true };
    }
}
