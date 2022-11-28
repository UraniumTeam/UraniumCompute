using Mono.Cecil.Cil;
using UraniumCompute.Compiler.Decompiling;

namespace UraniumCompute.Compiler.Syntax;

internal class BinaryExpressionSyntax : ExpressionSyntax
{
    internal BinaryOperationKind Kind { get; }
    internal ExpressionSyntax Left { get; }
    internal ExpressionSyntax Right { get; }
    public override TypeSymbol ExpressionType { get; }

    private static readonly BinaryOperationDesc[] definedOperations;

    static BinaryExpressionSyntax()
    {
        definedOperations = new[]
        {
            new BinaryOperationDesc(typeof(int), typeof(int), BinaryOperationKind.Add, typeof(int)),
            new BinaryOperationDesc(typeof(int), typeof(int), BinaryOperationKind.Sub, typeof(int)),
            new BinaryOperationDesc(typeof(int), typeof(int), BinaryOperationKind.Mul, typeof(int)),
            new BinaryOperationDesc(typeof(int), typeof(int), BinaryOperationKind.Div, typeof(int)),
            new BinaryOperationDesc(typeof(int), typeof(int), BinaryOperationKind.Mod, typeof(int)),

            new BinaryOperationDesc(typeof(uint), typeof(uint), BinaryOperationKind.Add, typeof(uint)),
            new BinaryOperationDesc(typeof(uint), typeof(uint), BinaryOperationKind.Sub, typeof(uint)),
            new BinaryOperationDesc(typeof(uint), typeof(uint), BinaryOperationKind.Mul, typeof(uint)),
            new BinaryOperationDesc(typeof(uint), typeof(uint), BinaryOperationKind.Div, typeof(uint)),
            new BinaryOperationDesc(typeof(uint), typeof(uint), BinaryOperationKind.Mod, typeof(uint)),

            new BinaryOperationDesc(typeof(int), typeof(uint), BinaryOperationKind.Add, typeof(int)),
            new BinaryOperationDesc(typeof(int), typeof(uint), BinaryOperationKind.Sub, typeof(int)),
            new BinaryOperationDesc(typeof(int), typeof(uint), BinaryOperationKind.Mul, typeof(int)),
            new BinaryOperationDesc(typeof(int), typeof(uint), BinaryOperationKind.Div, typeof(int)),
            new BinaryOperationDesc(typeof(int), typeof(uint), BinaryOperationKind.Mod, typeof(int)),

            new BinaryOperationDesc(typeof(uint), typeof(int), BinaryOperationKind.Add, typeof(int)),
            new BinaryOperationDesc(typeof(uint), typeof(int), BinaryOperationKind.Sub, typeof(int)),
            new BinaryOperationDesc(typeof(uint), typeof(int), BinaryOperationKind.Mul, typeof(int)),
            new BinaryOperationDesc(typeof(uint), typeof(int), BinaryOperationKind.Div, typeof(int)),
            new BinaryOperationDesc(typeof(uint), typeof(int), BinaryOperationKind.Mod, typeof(int)),

            new BinaryOperationDesc(typeof(float), typeof(float), BinaryOperationKind.Add, typeof(float)),
            new BinaryOperationDesc(typeof(float), typeof(float), BinaryOperationKind.Sub, typeof(float)),
            new BinaryOperationDesc(typeof(float), typeof(float), BinaryOperationKind.Mul, typeof(float)),
            new BinaryOperationDesc(typeof(float), typeof(float), BinaryOperationKind.Div, typeof(float)),

            new BinaryOperationDesc(null, null, BinaryOperationKind.Eq, typeof(bool)),
            new BinaryOperationDesc(null, null, BinaryOperationKind.Gt, typeof(bool)),
            new BinaryOperationDesc(null, null, BinaryOperationKind.Lt, typeof(bool))
        };
    }

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
