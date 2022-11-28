using System.Diagnostics.CodeAnalysis;
using Mono.Cecil;

namespace UraniumCompute.Compiler.Decompiling;

internal sealed class GenericTypeSymbol : TypeSymbol
{
    public override string FullName => $"{Name}<{Argument}>";

    public string Name { get; }
    public TypeSymbol Argument { get; }

    public GenericTypeSymbol(string name, TypeSymbol argument)
    {
        Name = name;
        Argument = argument;
    }

    public override bool TryGetFieldDesc(FieldReference field, [MaybeNullWhen(false)] out FieldDesc desc)
    {
        desc = null;
        return false;
    }
}
