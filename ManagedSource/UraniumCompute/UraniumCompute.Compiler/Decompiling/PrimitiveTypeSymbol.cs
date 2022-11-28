using System.Diagnostics.CodeAnalysis;
using Mono.Cecil;

namespace UraniumCompute.Compiler.Decompiling;

internal sealed class PrimitiveTypeSymbol : TypeSymbol
{
    public override string FullName { get; }

    public PrimitiveTypeSymbol(string name)
    {
        FullName = name;
    }

    public override bool TryGetFieldDesc(FieldReference field, [MaybeNullWhen(false)] out FieldDesc desc)
    {
        desc = null;
        return false;
    }
}
