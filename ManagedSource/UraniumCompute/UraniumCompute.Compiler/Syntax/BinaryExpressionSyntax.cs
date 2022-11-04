namespace UraniumCompute.Compiler.Syntax;

public class BinaryExpressionSyntax : ExpressionSyntax
{
    internal static readonly Dictionary<string, BinaryOperationKind> BinaryCilOperations = new()
    {
        { "add", BinaryOperationKind.Add },
        { "sub", BinaryOperationKind.Sub },
        { "mul", BinaryOperationKind.Mul },
        { "div", BinaryOperationKind.Div },
        { "div.un", BinaryOperationKind.Div },
        { "rem", BinaryOperationKind.Mod },
        { "rem.un", BinaryOperationKind.Mod },
        { "ceq", BinaryOperationKind.Eq },
        { "cgt", BinaryOperationKind.GreaterThan },
        { "cgt.un", BinaryOperationKind.GreaterThan },
        { "clt", BinaryOperationKind.LowerThan },
        { "clt.un", BinaryOperationKind.LowerThan },
        { "and", BinaryOperationKind.And },
        { "or", BinaryOperationKind.Or },
        { "shl", BinaryOperationKind.ShiftL },
        { "shr", BinaryOperationKind.ShiftR },
        { "shr.un", BinaryOperationKind.ShiftR },
        { "xor", BinaryOperationKind.Xor }
    };

    internal BinaryOperationKind Kind { get; }
    internal LiteralExpressionSyntax Left { get; }
    internal LiteralExpressionSyntax Right { get; }

    public BinaryExpressionSyntax(BinaryOperationKind kind, LiteralExpressionSyntax left, LiteralExpressionSyntax right)
    {
        Kind = kind;
        Left = left;
        Right = right;
    }
}