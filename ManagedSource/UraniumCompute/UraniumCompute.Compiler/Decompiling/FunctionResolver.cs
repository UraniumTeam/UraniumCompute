using System.Numerics;
using Mono.Cecil;
using UraniumCompute.Common.Math;

namespace UraniumCompute.Compiler.Decompiling;

internal static class FunctionResolver
{
    public static FunctionSymbol Resolve(MethodReference methodReference, Action<MethodReference> userFunctionCallback,
        Action<TypeReference> userTypeCallback)
    {
        if (methodReference.DeclaringType.Namespace == typeof(MathF).Namespace
            || methodReference.DeclaringType.Namespace == typeof(Matrix2x2).Namespace
            || methodReference.DeclaringType.Namespace == typeof(Matrix4x4).Namespace)
        {
            return IntrinsicFunctionSymbol.Resolve($"{methodReference.DeclaringType.Name}.{methodReference.Name}");
        }

        userFunctionCallback(methodReference);
        var returnType = TypeResolver.CreateType(methodReference.ReturnType, userTypeCallback);
        var arguments = methodReference.Parameters
            .Select(x => x.ParameterType)
            .Select(x => TypeResolver.CreateType(x, userTypeCallback));
        return new UserFunctionSymbol(methodReference.Name, returnType, arguments);
    }
}
