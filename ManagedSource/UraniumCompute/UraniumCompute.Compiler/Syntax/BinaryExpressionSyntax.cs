using System.Numerics;
using Mono.Cecil.Cil;
using UraniumCompute.Common.Math;
using UraniumCompute.Compiler.Decompiling;

namespace UraniumCompute.Compiler.Syntax;

internal class BinaryExpressionSyntax : ExpressionSyntax
{
    internal BinaryOperationKind Kind { get; }
    internal ExpressionSyntax Left { get; }
    internal ExpressionSyntax Right { get; }
    public override TypeSymbol ExpressionType { get; }

    private static readonly HashSet<BinaryOperationDesc> definedOperations;

    private static readonly Dictionary<string, BinaryOperationKind> operators = new()
    {
        { "op_Division", BinaryOperationKind.Div },
        { "op_Addition", BinaryOperationKind.Add },
        { "op_Subtraction", BinaryOperationKind.Sub },
        { "op_Multiply", BinaryOperationKind.Mul },
        { "op_Equality", BinaryOperationKind.Eq },
    };

    private static readonly List<Type> basicTypes = new()
    {
        typeof(int),
        typeof(uint),
        typeof(float)
    };

    private static readonly List<Type> numericsTypes = new()
    {
        typeof(Matrix2x2),
        typeof(Matrix3x3),
        typeof(Matrix4x4),
        typeof(Matrix2x2Int),
        typeof(Matrix3x3Int),
        typeof(Matrix4x4Int),
        typeof(Matrix2x2Uint),
        typeof(Matrix3x3Uint),
        typeof(Matrix4x4Uint),
        typeof(Vector2),
        typeof(Vector3),
        typeof(Vector4),
        typeof(Vector2Int),
        typeof(Vector3Int),
        typeof(Vector4Int),
        typeof(Vector2Uint),
        typeof(Vector3Uint),
        typeof(Vector4Uint)
    };

    internal BinaryExpressionSyntax(BinaryOperationKind kind, ExpressionSyntax right, ExpressionSyntax left)
    {
        var type = null as TypeSymbol;
        foreach (var operation in definedOperations)
        {
            if (operation.Match(left.ExpressionType, right.ExpressionType, kind))
            {
                type = operation.ResultType;
                break;
            }
        }

        ExpressionType = type ?? throw new ArgumentException("Unknown operation on types: " +
                                                             $"{left.ExpressionType} and {right.ExpressionType}");

        Kind = kind;
        Left = left;
        Right = right;

        if (Left.ExpressionType.FullName != "bool" && Right.ExpressionType.FullName != "bool")
        {
            return;
        }

        if (Left is LiteralExpressionSyntax { Value: not bool } l)
        {
            Left = new LiteralExpressionSyntax(Convert.ToBoolean(l.Value));
        }

        if (Right is LiteralExpressionSyntax { Value: not bool } r)
        {
            Right = new LiteralExpressionSyntax(Convert.ToBoolean(r.Value));
        }
    }

    static BinaryExpressionSyntax()
    {
        definedOperations = new HashSet<BinaryOperationDesc>
        {
            new(typeof(int), typeof(int), BinaryOperationKind.Mod, typeof(int)),
            new(typeof(uint), typeof(uint), BinaryOperationKind.Mod, typeof(uint)),

            new(typeof(int), typeof(uint), BinaryOperationKind.Add, typeof(int)),
            new(typeof(int), typeof(uint), BinaryOperationKind.Sub, typeof(int)),
            new(typeof(int), typeof(uint), BinaryOperationKind.Mul, typeof(int)),
            new(typeof(int), typeof(uint), BinaryOperationKind.Div, typeof(int)),
            new(typeof(int), typeof(uint), BinaryOperationKind.Mod, typeof(int)),

            new(typeof(uint), typeof(int), BinaryOperationKind.Add, typeof(int)),
            new(typeof(uint), typeof(int), BinaryOperationKind.Sub, typeof(int)),
            new(typeof(uint), typeof(int), BinaryOperationKind.Mul, typeof(int)),
            new(typeof(uint), typeof(int), BinaryOperationKind.Div, typeof(int)),
            new(typeof(uint), typeof(int), BinaryOperationKind.Mod, typeof(int)),

            new(null, null, BinaryOperationKind.Eq, typeof(bool)),
            new(null, null, BinaryOperationKind.Gt, typeof(bool)),
            new(null, null, BinaryOperationKind.Lt, typeof(bool)), 
            
            new(typeof(Vector2), typeof(Vector2), BinaryOperationKind.Div, typeof(Vector2)),
            new(typeof(Vector3), typeof(Vector3), BinaryOperationKind.Div, typeof(Vector3)),
            new(typeof(Vector4), typeof(Vector4), BinaryOperationKind.Div, typeof(Vector4)),
            new(typeof(Vector2Int), typeof(Vector2Int), BinaryOperationKind.Div, typeof(Vector2Int)),
            new(typeof(Vector3Int), typeof(Vector3Int), BinaryOperationKind.Div, typeof(Vector3Int)),
            new(typeof(Vector4Int), typeof(Vector4Int), BinaryOperationKind.Div, typeof(Vector4Int)),
            new(typeof(Vector2Uint), typeof(Vector2Uint), BinaryOperationKind.Div, typeof(Vector2Uint)),
            new(typeof(Vector3Uint), typeof(Vector3Uint), BinaryOperationKind.Div, typeof(Vector3Uint)),
            new(typeof(Vector4Uint), typeof(Vector4Uint), BinaryOperationKind.Div, typeof(Vector4Uint)),
        };

        foreach (var type in basicTypes)
        {
            definedOperations.Add(new BinaryOperationDesc(type, type, BinaryOperationKind.Add, type));
            definedOperations.Add(new BinaryOperationDesc(type, type, BinaryOperationKind.Sub, type));
            definedOperations.Add(new BinaryOperationDesc(type, type, BinaryOperationKind.Mul, type));
            definedOperations.Add(new BinaryOperationDesc(type, type, BinaryOperationKind.Div, type));
        }

        foreach (var type in numericsTypes)
        foreach (var operation in operators.Skip(1))
        {
            AddOverloadedOperator(type, operation.Key, operation.Value);
            definedOperations.Add(new BinaryOperationDesc(type, typeof(int), operation.Value, type));
            definedOperations.Add(new BinaryOperationDesc(typeof(int), type, operation.Value, type));
        }
    }

    private static void AddOverloadedOperator(Type type, string name, BinaryOperationKind kind)
    {
        var methods = type.GetMethods().Where(m => m.Name == name).ToList();
        if (methods.Count == 0)
            throw new ArgumentException();
        var returnType = methods[0].ReturnType;
        foreach (var method in methods)
        {
            var paramTypes = method.GetParameters().Select(x => x.ParameterType).ToList();
            definedOperations.Add(
                new BinaryOperationDesc(paramTypes[0], paramTypes[1], kind, returnType));
        }
    }

    internal static BinaryOperationKind GetOperationKind(Code code)
    {
        return code switch
        {
            Code.Add => BinaryOperationKind.Add,
            Code.Sub => BinaryOperationKind.Sub,
            Code.Mul => BinaryOperationKind.Mul,
            Code.Div or Code.Div_Un => BinaryOperationKind.Div,
            Code.Rem or Code.Rem_Un => BinaryOperationKind.Mod,
            Code.Ceq => BinaryOperationKind.Eq,
            Code.Cgt or Code.Cgt_Un => BinaryOperationKind.Gt,
            Code.Clt or Code.Clt_Un => BinaryOperationKind.Lt,
            Code.And => BinaryOperationKind.And,
            Code.Or => BinaryOperationKind.Or,
            Code.Shl => BinaryOperationKind.ShiftL,
            Code.Shr or Code.Shr_Un => BinaryOperationKind.ShiftR,
            Code.Xor => BinaryOperationKind.Xor,
            _ => BinaryOperationKind.None
        };
    }

    internal static BinaryOperationKind GetOperationKind(string name)
    {
        return operators.TryGetValue(name, out var value) ? value : BinaryOperationKind.None;
    }

    internal static string GetOperationString(BinaryOperationKind kind)
    {
        return kind switch
        {
            BinaryOperationKind.Add => "+",
            BinaryOperationKind.Sub => "-",
            BinaryOperationKind.Mul => "*",
            BinaryOperationKind.Div => "/",
            BinaryOperationKind.Mod => "%",
            BinaryOperationKind.Eq => "==",
            BinaryOperationKind.Gt => ">",
            BinaryOperationKind.Lt => "<",
            BinaryOperationKind.And => "&",
            BinaryOperationKind.Or => "|",
            BinaryOperationKind.ShiftL => "<<",
            BinaryOperationKind.ShiftR => ">>",
            BinaryOperationKind.Xor => "^",
            BinaryOperationKind.None => "",
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
        };
    }

    public override string ToString()
    {
        return $"({Left} {GetOperationString(Kind)} {Right})";
    }
}
