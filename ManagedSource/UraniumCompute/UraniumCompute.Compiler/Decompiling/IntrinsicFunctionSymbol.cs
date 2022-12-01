﻿namespace UraniumCompute.Compiler.Decompiling;

internal sealed class IntrinsicFunctionSymbol : FunctionSymbol
{
    public override string FullName { get; }
    public override TypeSymbol ReturnType { get; }
    public override TypeSymbol[] ArgumentTypes { get; }

    private static readonly Dictionary<string, IntrinsicFunctionSymbol> functions;

    static IntrinsicFunctionSymbol()
    {
        (string, IntrinsicFunctionSymbol) CreateIntrinsic(Delegate d)
        {
            var method = d.Method;
            var returnType = TypeResolver.CreateType(method.ReturnType, _ => { });
            var arguments = method.GetParameters()
                .Select(x => x.ParameterType)
                .Select(x => TypeResolver.CreateType(x, _ => { }));
            var symbol = new IntrinsicFunctionSymbol(method.Name.ToLower(), returnType, arguments);
            return ($"{method.DeclaringType!.Name}.{method.Name}", symbol);
        }

        functions = Enumerable.Empty<Delegate>()
            .Append(MathF.Sin)
            .Append(MathF.Cos)
            .Append(MathF.Min)
            .Append(MathF.Max)
            .Select(CreateIntrinsic)
            .ToDictionary(x => x.Item1, x => x.Item2);
    }

    private IntrinsicFunctionSymbol(string fullName, TypeSymbol returnType, IEnumerable<TypeSymbol> argumentTypes)
    {
        FullName = fullName;
        ReturnType = returnType;
        ArgumentTypes = argumentTypes.ToArray();
    }

    public static IntrinsicFunctionSymbol Resolve(string name)
    {
        return functions[name];
    }
}
