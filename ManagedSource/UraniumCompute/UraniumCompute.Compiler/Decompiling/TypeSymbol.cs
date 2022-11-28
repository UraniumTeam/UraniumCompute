using System.Diagnostics.CodeAnalysis;
using Mono.Cecil;

namespace UraniumCompute.Compiler.Decompiling;

internal abstract class TypeSymbol
{
    public abstract string FullName { get; }

    public abstract bool TryGetFieldDesc(FieldReference field, [MaybeNullWhen(false)] out FieldDesc desc);

    public override string ToString()
    {
        return FullName;
    }

    protected bool Equals(TypeSymbol other)
    {
        return FullName == other.FullName;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((TypeSymbol)obj);
    }

    public override int GetHashCode()
    {
        return FullName.GetHashCode();
    }

    public static bool operator ==(TypeSymbol? left, TypeSymbol? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(TypeSymbol? left, TypeSymbol? right)
    {
        return !Equals(left, right);
    }
}
