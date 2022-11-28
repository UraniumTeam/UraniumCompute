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
        Left = left is null ? null : TypeResolver.CreateType(left, _ => { });
        Right = right is null ? null : TypeResolver.CreateType(right, _ => { });
        Kind = kind;
        ResultType = TypeResolver.CreateType(resultType, _ => { });
    }

    public bool Match(TypeSymbol left, TypeSymbol right, BinaryOperationKind kind)
    {
        return (left == Left || Left is null)
               && (right == Right || Right is null)
               && kind == Kind;
    }
}
