using Mono.Cecil;

namespace UraniumCompute.Compiler.Decompiling;

internal static class FunctionResolver
{
    public static FunctionSymbol Resolve(MethodReference methodReference, Action<MethodReference> userFunctionCallback)
    {
        if (methodReference.DeclaringType.Namespace == typeof(Math).Namespace)
        {
            return IntrinsicFunctionSymbol.Resolve($"{methodReference.DeclaringType.Name}.{methodReference.Name}");
        }

        userFunctionCallback(methodReference);
        var returnType = TypeResolver.CreateType(methodReference.ReturnType);
        var arguments = methodReference.Parameters
            .Select(x => x.ParameterType)
            .Select(TypeResolver.CreateType);
        return new UserFunctionSymbol(methodReference.Name, returnType, arguments);
    }
}
