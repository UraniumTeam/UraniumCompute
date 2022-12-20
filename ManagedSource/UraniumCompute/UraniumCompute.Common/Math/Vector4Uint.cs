using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace UraniumCompute.Common.Math;

[DeviceType("uint4")]
[StructLayout(LayoutKind.Explicit)]
public struct Vector4Uint : IEquatable<Vector4Uint>
{
    public static Vector4Uint Zero => default;

    public static Vector4Uint One => new(1);

    public static Vector4Uint UnitX => new(1, 0, 0, 0);
    public static Vector4Uint UnitY => new(0, 1, 0, 0);
    public static Vector4Uint UnitZ => new(0, 0, 1, 0);
    public static Vector4Uint UnitW => new(0, 0, 0, 1);
    [FieldOffset(0)] public uint X;

    [FieldOffset(sizeof(uint))] public uint Y;

    [FieldOffset(sizeof(uint) * 2)] public uint Z;

    [FieldOffset(sizeof(uint) * 3)] public uint W;

    [FieldOffset(0)] private readonly Vector128<uint> value;

    private static readonly unsafe int size = sizeof(Vector4Uint);

    public uint this[int index]
    {
        get
        {
            if ((uint)index >= 4)
            {
                throw new ArgumentOutOfRangeException();
            }

            return Unsafe.Add(ref Unsafe.As<Vector4Uint, uint>(ref this), index);
        }
        set
        {
            if ((uint)index >= 4)
            {
                throw new ArgumentOutOfRangeException();
            }

            Unsafe.Add(ref Unsafe.As<Vector4Uint, uint>(ref this), index) = value;
        }
    }
    
    public Vector4Uint(ReadOnlySpan<uint> values)
    {
        if (values.Length < 4)
        {
            throw new ArgumentException($"{nameof(values)} must be at least 4 elements in length");
        }

        this = Unsafe.ReadUnaligned<Vector4Uint>(ref Unsafe.As<uint, byte>(ref MemoryMarshal.GetReference(values)));
    }

    public Vector4Uint(uint value) : this(stackalloc[] { value, value, value, value })
    {
    }

    public Vector4Uint(uint x, uint y, uint z, uint w) : this(stackalloc[] { x, y, z, w })
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4Uint operator +(Vector4Uint left, Vector4Uint right)
    {
        var vec = left.value + right.value;
        return Unsafe.ReadUnaligned<Vector4Uint>(ref Unsafe.As<Vector128<uint>, byte>(ref vec));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4Uint operator -(Vector4Uint left, Vector4Uint right)
    {
        var vec = left.value - right.value;
        return Unsafe.ReadUnaligned<Vector4Uint>(ref Unsafe.As<Vector128<uint>, byte>(ref vec));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4Uint operator *(Vector4Uint left, Vector4Uint right)
    {
        var vec = left.value * right.value;
        return Unsafe.ReadUnaligned<Vector4Uint>(ref Unsafe.As<Vector128<uint>, byte>(ref vec));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4Uint operator /(Vector4Uint left, Vector4Uint right)
    {
        var vec = left.value / right.value;
        return Unsafe.ReadUnaligned<Vector4Uint>(ref Unsafe.As<Vector128<uint>, byte>(ref vec));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2Int operator /(Vector4Uint left, uint right)
    {
        var vec = left.value / Vector128.Create(right);
        return Unsafe.ReadUnaligned<Vector2Int>(ref Unsafe.As<Vector128<uint>, byte>(ref vec));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4Uint operator *(Vector4Uint left, uint right)
    {
        var vec = left.value * right;
        return Unsafe.ReadUnaligned<Vector4Uint>(ref Unsafe.As<Vector128<uint>, byte>(ref vec));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4Uint operator *(uint left, Vector4Uint right)
    {
        var vec = right.value * left;
        return Unsafe.ReadUnaligned<Vector4Uint>(ref Unsafe.As<Vector128<uint>, byte>(ref vec));
    }

    public bool Equals(Vector4Uint other)
    {
        return value.Equals(other.value);
    }

    public override bool Equals(object? obj)
    {
        return obj is Vector4Uint other && Equals(other);
    }

    public override int GetHashCode()
    {
        return value.GetHashCode();
    }

    public static bool operator ==(Vector4Uint left, Vector4Uint right)
    {
        return left.value == right.value;
    }

    public static bool operator !=(Vector4Uint left, Vector4Uint right)
    {
        return left.value != right.value;
    }

    public override string ToString()
    {
        return $"[{X}, {Y}, {Z}, {W}]";
    }
}
