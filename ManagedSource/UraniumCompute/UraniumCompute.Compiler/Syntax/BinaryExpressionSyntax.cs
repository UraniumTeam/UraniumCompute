using Mono.Cecil.Cil;

namespace UraniumCompute.Compiler.Syntax;

public class BinaryExpressionSyntax : ExpressionSyntax
{
    internal BinaryOperationKind Kind { get; }
    internal ExpressionSyntax Left { get; }
    internal ExpressionSyntax Right { get; }

    public BinaryExpressionSyntax(BinaryOperationKind kind, ExpressionSyntax left, ExpressionSyntax right)
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
}
