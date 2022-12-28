using System.Diagnostics.CodeAnalysis;
using Mono.Cecil;

namespace UraniumCompute.Compiler.Decompiling;

internal sealed class RefTypeSymbol : TypeSymbol
{
    public override string FullName => $"inout {BaseType.FullName}";
    public TypeSymbol BaseType { get; }

    public RefTypeSymbol(TypeSymbol baseType)
    {
        BaseType = baseType;
    }

    public override bool TryGetFieldDesc(FieldReference field, [MaybeNullWhen(false)] out FieldDesc desc)
    {
        return BaseType.TryGetFieldDesc(field, out desc);
    }
}
