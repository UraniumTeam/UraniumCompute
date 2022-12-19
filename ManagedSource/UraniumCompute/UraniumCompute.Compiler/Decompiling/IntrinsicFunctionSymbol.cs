﻿using System.Numerics;
using System.Reflection;
using UraniumCompute.Common.Math;

namespace UraniumCompute.Compiler.Decompiling;

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

        functions = Enumerable.Empty<Delegate>()
            .Append(MathF.Sin)
            .Append(MathF.Cos)
            .Append(MathF.Min)
            .Append(MathF.Max)
            .Append(MathF.Abs)
            .Append(MathF.Acos)
            .Append(MathF.Asin)
            .Append(Matrix2x2.Transpose)
            .Append(Matrix2x2Int.Transpose)
            .Append(Matrix2x2Uint.Transpose)
            .Append(Matrix3x3.Transpose)
            .Append(Matrix3x3Int.Transpose)
            .Append(Matrix3x3Uint.Transpose)
            .Append(Matrix4x4.Transpose)
            .Append(Matrix4x4Int.Transpose)
            .Append(Matrix4x4Uint.Transpose)
            .Select(CreateIntrinsic)
            .ToDictionary(x => x.Item1, x => x.Item2);

        CreateMemberIntrinsic<Matrix2x2>(nameof(Matrix2x2.GetDeterminant), "determinant");
        CreateMemberIntrinsic<Matrix3x3>(nameof(Matrix2x2.GetDeterminant), "determinant");
        CreateMemberIntrinsic<Matrix4x4>(nameof(Matrix2x2.GetDeterminant), "determinant");
        CreateMemberIntrinsic<Matrix2x2Int>(nameof(Matrix2x2.GetDeterminant), "determinant");
        CreateMemberIntrinsic<Matrix3x3Int>(nameof(Matrix2x2.GetDeterminant), "determinant");
        CreateMemberIntrinsic<Matrix4x4Int>(nameof(Matrix2x2.GetDeterminant), "determinant");
        CreateMemberIntrinsic<Matrix2x2Uint>(nameof(Matrix2x2.GetDeterminant), "determinant");
        CreateMemberIntrinsic<Matrix3x3Uint>(nameof(Matrix2x2.GetDeterminant), "determinant");
        CreateMemberIntrinsic<Matrix4x4Uint>(nameof(Matrix2x2.GetDeterminant), "determinant");

        CreateMemberCtorIntrinsic<Matrix2x2>();
        CreateMemberCtorIntrinsic<Matrix3x3>();
        CreateMemberCtorIntrinsic<Matrix4x4>();
        CreateMemberCtorIntrinsic<Matrix2x2Int>();
        CreateMemberCtorIntrinsic<Matrix3x3Int>();
        CreateMemberCtorIntrinsic<Matrix4x4Int>();
        CreateMemberCtorIntrinsic<Matrix2x2Uint>();
        CreateMemberCtorIntrinsic<Matrix3x3Uint>();
        CreateMemberCtorIntrinsic<Matrix4x4Uint>();
        CreateMemberCtorIntrinsic<Vector2>();
        CreateMemberCtorIntrinsic<Vector3>();
        CreateMemberCtorIntrinsic<Vector4>();
        CreateMemberCtorIntrinsic<Vector2Int>();
        CreateMemberCtorIntrinsic<Vector3Int>();
        CreateMemberCtorIntrinsic<Vector4Int>();
        CreateMemberCtorIntrinsic<Vector2Uint>();
        CreateMemberCtorIntrinsic<Vector3Uint>();
        CreateMemberCtorIntrinsic<Vector4Uint>();
        
        
        var m = new Matrix4x4();
        var v = new Vector4();
        Matrix4x4.Lerp(m,m,5);
        MathF.Acos(5.0f);
        v.Length();
        Vector4.Dot(v, v);
    }

    private static void CreateMemberIntrinsic<T>(string name, string? hlslName = null)
    {
        var method = typeof(T).GetMethod(name) ?? throw new ArgumentException();
        hlslName ??= method.Name.ToLower();
        var returnType = TypeResolver.CreateType(method.ReturnType, _ => { });
        var arguments = new List<TypeSymbol>();
        if (method.CallingConvention >= CallingConventions.HasThis)
            arguments.Add(TypeResolver.CreateType(typeof(T), _ => { }));
        arguments.AddRange(
            method.GetParameters()
                .Select(x => x.ParameterType)
                .Select(x => TypeResolver.CreateType(x, _ => { })));
        var symbol = new IntrinsicFunctionSymbol(hlslName, returnType, arguments);
        functions[$"{method.DeclaringType!.Name}.{method.Name}"] = symbol;
    }

    private static void CreateMemberCtorIntrinsic<T>()
    {
        var constructors = typeof(T).GetConstructors() ?? throw new ArgumentException();
        var declaringType = constructors[0].DeclaringType!;
        var returnType = TypeResolver.CreateType(declaringType, _ => { });
        var hlslName = returnType.FullName.ToLower();
        var arguments = new List<TypeSymbol>();
        foreach (var constructor in constructors)
        {
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
            functions[$"{declaringType.Name}..ctor"] = symbol;
        }
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
