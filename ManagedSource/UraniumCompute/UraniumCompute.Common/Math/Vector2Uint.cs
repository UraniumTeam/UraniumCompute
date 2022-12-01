using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace UraniumCompute.Common.Math;

[DeviceType("uint2")]
[StructLayout(LayoutKind.Explicit)]
public struct Vector2Uint : IEquatable<Vector2Uint>
{
    public static Vector2Uint Zero => default;

    public static Vector2Uint One => new(1);

    public static Vector2Uint UnitX => new(1, 0);
    public static Vector2Uint UnitY => new(0, 1);
    [FieldOffset(0)] public uint X;

    [FieldOffset(sizeof(uint))] public uint Y;

    [FieldOffset(0)] private readonly Vector64<uint> value;

    private static readonly unsafe int size = sizeof(Vector2Uint);

    public uint this[int index]
    {
        get
        {
            if ((uint)index >= 2)
            {
                throw new ArgumentOutOfRangeException();
            }

            return Unsafe.Add(ref Unsafe.As<Vector2Uint, uint>(ref this), index);
        }
        set
        {
            if ((uint)index >= 2)
            {
                throw new ArgumentOutOfRangeException();
            }

            Unsafe.Add(ref Unsafe.As<Vector2Uint, uint>(ref this), index) = value;
        }
    }

    public Vector2Uint(ReadOnlySpan<uint> values)
    {
        if (values.Length < 2)
        {
            throw new ArgumentException($"{nameof(values)} must be at least 2 elements in length");
        }

        this = Unsafe.ReadUnaligned<Vector2Uint>(ref Unsafe.As<uint, byte>(ref MemoryMarshal.GetReference(values)));
    }

    public Vector2Uint(uint value) : this(stackalloc[] { value, value })
    {
    }

    public Vector2Uint(uint x, uint y) : this(stackalloc[] { x, y })
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2Uint operator +(Vector2Uint left, Vector2Uint right)
    {
        var vec = left.value + right.value;
        return Unsafe.ReadUnaligned<Vector2Uint>(ref Unsafe.As<Vector64<uint>, byte>(ref vec));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2Uint operator -(Vector2Uint left, Vector2Uint right)
    {
        var vec = left.value - right.value;
        return Unsafe.ReadUnaligned<Vector2Uint>(ref Unsafe.As<Vector64<uint>, byte>(ref vec));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2Uint operator *(Vector2Uint left, Vector2Uint right)
    {
        var vec = left.value * right.value;
        return Unsafe.ReadUnaligned<Vector2Uint>(ref Unsafe.As<Vector64<uint>, byte>(ref vec));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2Uint operator /(Vector2Uint left, Vector2Uint right)
    {
        var vec = left.value / right.value;
        return Unsafe.ReadUnaligned<Vector2Uint>(ref Unsafe.As<Vector64<uint>, byte>(ref vec));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2Uint operator *(Vector2Uint left, uint right)
    {
        var vec = left.value * right;
        return Unsafe.ReadUnaligned<Vector2Uint>(ref Unsafe.As<Vector64<uint>, byte>(ref vec));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2Uint operator *(uint left, Vector2Uint right)
    {
        var vec = right.value * left;
        return Unsafe.ReadUnaligned<Vector2Uint>(ref Unsafe.As<Vector64<uint>, byte>(ref vec));
    }

    public bool Equals(Vector2Uint other)
    {
        return value.Equals(other.value);
    }

    public override bool Equals(object? obj)
    {
        return obj is Vector2Uint other && Equals(other);
    }

    public override int GetHashCode()
    {
        return value.GetHashCode();
    }

    public static bool operator ==(Vector2Uint left, Vector2Uint right)
    {
        return left.value == right.value;
    }

    public static bool operator !=(Vector2Uint left, Vector2Uint right)
    {
        return left.value != right.value;
    }

    public override string ToString()
    {
        return $"[{X}, {Y}]";
    }
}
