using UraniumCompute.Compiler.Decompiling;

namespace UraniumCompute.Compiler.Syntax;

internal struct BinaryOperationDesc
{
    public TypeSymbol? Left { get; }
    public TypeSymbol? Right { get; }
    public BinaryOperationKind Kind { get; }
    public TypeSymbol ResultType { get; }

    public BinaryOperationDesc(Type? left, Type? right, BinaryOperationKind kind, Type resultType)
    {
        Left = left is null ? null : TypeResolver.CreateType(left);
        Right = right is null ? null : TypeResolver.CreateType(right);
        Kind = kind;
        ResultType = TypeResolver.CreateType(resultType);
    }

    public bool Match(TypeSymbol left, TypeSymbol right, BinaryOperationKind kind)
    {
        return (left == Left || Left is null)
               && (right == Right || Right is null)
               && kind == Kind;
    }
}
