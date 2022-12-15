using System.Numerics;
using System.Runtime.InteropServices;

namespace UraniumCompute.Acceleration.Allocators;

[StructLayout(LayoutKind.Sequential)]
public readonly struct NullableHandle
    : IComparable<NullableHandle>, IComparable, IEquatable<NullableHandle>,
        IAdditionOperators<NullableHandle, ulong, NullableHandle>,
        ISubtractionOperators<NullableHandle, ulong, NullableHandle>,
        ISubtractionOperators<NullableHandle, NullableHandle, long>,
        IComparisonOperators<NullableHandle, NullableHandle, bool>,
        IDecrementOperators<NullableHandle>,
        IIncrementOperators<NullableHandle>
{
    public static readonly NullableHandle Null = new();
    public static readonly NullableHandle Zero = new(0);

    public bool IsNull => address == nullAddress;
    public bool IsValid => address != nullAddress;

    private readonly ulong address;

    private const ulong nullAddress = ulong.MaxValue;

    public NullableHandle()
    {
        address = nullAddress;
    }

    public static NullableHandle FromOffset(ulong offset)
    {
        return new NullableHandle(offset);
    }

    public static NullableHandle FromOffset(long offset)
    {
        return new NullableHandle((ulong)offset);
    }

    public static NullableHandle FromIntPtr(IntPtr ptr)
    {
        return FromOffset(ptr.ToInt64());
    }

    public static NullableHandle FromUIntPtr(UIntPtr ptr)
    {
        return FromOffset(ptr.ToUInt64());
    }

    private NullableHandle(ulong address)
    {
        this.address = address;
    }

    public NullableHandle AlignUp(ulong align)
    {
        return new NullableHandle((address + (align - 1u)) & ~(align - 1u));
    }

    public bool Equals(NullableHandle other)
    {
        return address == other.address;
    }

    public override bool Equals(object? obj)
    {
        return obj is NullableHandle other && Equals(other);
    }

    public override int GetHashCode()
    {
        return address.GetHashCode();
    }

    public static bool operator ==(NullableHandle left, NullableHandle right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(NullableHandle left, NullableHandle right)
    {
        return !left.Equals(right);
    }

    public int CompareTo(NullableHandle other)
    {
        return address.CompareTo(other.address);
    }

    public int CompareTo(object? obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return 1;
        }

        return obj is NullableHandle other
            ? CompareTo(other)
            : throw new ArgumentException($"Object must be of type {nameof(NullableHandle)}");
    }

    public static bool operator <(NullableHandle left, NullableHandle right)
    {
        return left.CompareTo(right) < 0;
    }

    public static bool operator >(NullableHandle left, NullableHandle right)
    {
        return left.CompareTo(right) > 0;
    }

    public static bool operator <=(NullableHandle left, NullableHandle right)
    {
        return left.CompareTo(right) <= 0;
    }

    public static bool operator >=(NullableHandle left, NullableHandle right)
    {
        return left.CompareTo(right) >= 0;
    }

    public static NullableHandle operator +(NullableHandle left, ulong right)
    {
        return new NullableHandle(left.address + right);
    }

    public static NullableHandle operator -(NullableHandle left, ulong right)
    {
        return new NullableHandle(left.address - right);
    }

    public static NullableHandle operator --(NullableHandle value)
    {
        return new NullableHandle(value.address - 1);
    }

    public static NullableHandle operator ++(NullableHandle value)
    {
        return new NullableHandle(value.address + 1);
    }

    public static explicit operator ulong(NullableHandle value)
    {
        return value.address;
    }

    public static explicit operator int(NullableHandle value)
    {
        return (int)value.address;
    }

    public static long operator -(NullableHandle left, NullableHandle right)
    {
        return (long)left.address - (long)right.address;
    }

    public override string ToString()
    {
        return IsNull ? "<null>" : $"0x{address:x8}";
    }
}
