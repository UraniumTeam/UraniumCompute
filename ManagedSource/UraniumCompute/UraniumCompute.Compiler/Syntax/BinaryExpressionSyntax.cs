using Mono.Cecil.Cil;

namespace UraniumCompute.Compiler.Syntax;

internal class BinaryExpressionSyntax : ExpressionSyntax
{
    internal BinaryOperationKind Kind { get; }
    internal ExpressionSyntax Left { get; }
    internal ExpressionSyntax Right { get; }

    internal BinaryExpressionSyntax(BinaryOperationKind kind, ExpressionSyntax right, ExpressionSyntax left)
    {
        Kind = kind;
        Left = left;
        Right = right;
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
            Code.Cgt or Code.Cgt_Un => BinaryOperationKind.GreaterThan,
            Code.Clt or Code.Clt_Un => BinaryOperationKind.LowerThan,
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
            BinaryOperationKind.GreaterThan => ">",
            BinaryOperationKind.LowerThan => "<",
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
