using System.Numerics;
using System.Reflection;
using UraniumCompute.Common.Math;

namespace UraniumCompute.Compiler.Decompiling;

internal sealed class IntrinsicFunctionSymbol : FunctionSymbol
{
    public override string FullName { get; }
    public override TypeSymbol ReturnType { get; }
    public override TypeSymbol[] ArgumentTypes { get; }

    private static readonly Dictionary<(string, int), IntrinsicFunctionSymbol> functions;

    static IntrinsicFunctionSymbol()
    {
        functions = Enumerable.Empty<Delegate>()
            .Append(MathF.Sin)
            .Append(MathF.Cos)
            .Append(MathF.Min)
            .Append(MathF.Max)
            .Append(MathF.Abs)
            .Append(MathF.Acos)
            .Append(MathF.Atan)
            .Append(MathF.Atan2)
            .Append(MathF.Cosh)
            .Append(MathF.Exp)
            .Append(MathF.Log2)
            .Append(MathF.Log10)
            .Append(MathF.Sinh)
            .Append(MathF.Sqrt)
            .Append(MathF.Tan)
            .Append(MathF.Tanh)
            .Append(MathF.Pow)
            .Append(Matrix2x2.Transpose)
            .Append(Matrix2x2Int.Transpose)
            .Append(Matrix2x2Uint.Transpose)
            .Append(Matrix3x3.Transpose)
            .Append(Matrix3x3Int.Transpose)
            .Append(Matrix3x3Uint.Transpose)
            .Append(Matrix4x4.Transpose)
            .Append(Matrix4x4Int.Transpose)
            .Append(Matrix4x4Uint.Transpose)
            .Append(Vector2.Dot)
            .Append(Vector2Int.Dot)
            .Append(Vector2Uint.Dot)
            .Append(Vector3.Dot)
            .Append(Vector3Int.Dot)
            .Append(Vector3Uint.Dot)
            .Append(Vector4.Dot)
            .Append(Vector4Int.Dot)
            .Append(Vector4Uint.Dot)
            .Append(Vector2.Normalize)
            .Append(Vector3.Normalize)
            .Append(Vector4.Normalize)
            .Append(Vector3.Cross)
            .Append(Vector3.Reflect)
            .Append(Vector2.Distance)
            .Append(Vector3.Distance)
            .Append(Vector4.Distance)
            .Select(CreateIntrinsic)
            .ToDictionary(x => (x.Item1, x.Item2.ArgumentTypes.Length), x => x.Item2);

        foreach (var type in TypeResolver.SupportedMatrixTypes)
        {
            CreateMemberCtorIntrinsic(type);
            CreateMemberIntrinsic(type, nameof(Matrix2x2.GetDeterminant), "determinant");
        }

        CreateMemberIntrinsic(typeof(Vector2), nameof(Vector2.Length), "length");
        CreateMemberIntrinsic(typeof(Vector3), nameof(Vector3.Length), "length");
        CreateMemberIntrinsic(typeof(Vector4), nameof(Vector4.Length), "length");

        foreach (var type in TypeResolver.SupportedVectorTypes)
        {
            CreateMemberCtorIntrinsic(type);
        }
    }

    private static (string, IntrinsicFunctionSymbol) CreateIntrinsic(Delegate d)
    {
        var method = d.Method;
        var returnType = TypeResolver.CreateType(method.ReturnType, _ => { });
        var arguments = new List<TypeSymbol>();
        if (method.CallingConvention >= CallingConventions.HasThis)
            arguments.Add(TypeResolver.CreateType(method.GetType(), _ => { }));
        arguments.AddRange(
            method.GetParameters()
                .Select(x => x.ParameterType)
                .Select(x => TypeResolver.CreateType(x, _ => { })));
        var symbol = new IntrinsicFunctionSymbol(method.Name.ToLower(), returnType, arguments);
        return ($"{method.DeclaringType!.Name}.{method.Name}", symbol);
    }

    private static void CreateMemberIntrinsic(Type type, string name, string? hlslName = null)
    {
        var methods = type.GetMethods().Where(m => m.Name == name).ToList();
        if (methods.Count == 0)
            throw new ArgumentException();
        hlslName ??= methods[0].Name.ToLower();
        var returnType = TypeResolver.CreateType(methods[0].ReturnType, _ => { });
        foreach (var method in methods)
        {
            var arguments = new List<TypeSymbol>();
            if (method.CallingConvention >= CallingConventions.HasThis)
                arguments.Add(TypeResolver.CreateType(type, _ => { }));
            var paramTypes = method.GetParameters().Select(x => x.ParameterType).ToList();
            arguments.AddRange(paramTypes.Select(x => TypeResolver.CreateType(x, _ => { })));
            var symbol = new IntrinsicFunctionSymbol(hlslName, returnType, arguments);
            functions[($"{method.DeclaringType!.Name}.{method.Name}", symbol.ArgumentTypes.Length - 1)] = symbol;
        }
    }

    private static void CreateMemberCtorIntrinsic(Type type)
    {
        var constructors = type.GetConstructors() ?? throw new ArgumentException();
        var declaringType = constructors[0].DeclaringType!;
        var returnType = TypeResolver.CreateType(declaringType, _ => { });
        var hlslName = returnType.FullName.ToLower();
        foreach (var constructor in constructors)
        {
            var arguments = new List<TypeSymbol>();
            var paramTypes = constructor.GetParameters().Select(x => x.ParameterType).ToList();
            // todo
            if (paramTypes.Contains(typeof(ReadOnlySpan<float>))
                || paramTypes.Contains(typeof(ReadOnlySpan<int>))
                || paramTypes.Contains(typeof(ReadOnlySpan<uint>))
                || paramTypes.Contains(typeof(Matrix3x2)))
                continue;
            //
            arguments.AddRange(paramTypes.Select(x => TypeResolver.CreateType(x, _ => { })));
            var symbol = new IntrinsicFunctionSymbol(hlslName, returnType, arguments);
            functions[($"{declaringType.Name}..ctor", symbol.ArgumentTypes.Length)] = symbol;
        }
    }

    private IntrinsicFunctionSymbol(string fullName, TypeSymbol returnType, IEnumerable<TypeSymbol> argumentTypes)
    {
        FullName = fullName;
        ReturnType = returnType;
        ArgumentTypes = argumentTypes.ToArray();
    }

    public static IntrinsicFunctionSymbol Resolve(string name, int argsCount)
    {
        return functions[(name, argsCount)];
    }
}
