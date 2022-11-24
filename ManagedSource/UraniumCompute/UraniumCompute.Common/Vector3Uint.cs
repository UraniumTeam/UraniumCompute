using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace UraniumCompute.Common;

[StructLayout(LayoutKind.Explicit)]
public struct Vector3Uint : IEquatable<Vector3Uint>
{
    [FieldOffset(0)] public uint X;

    [FieldOffset(sizeof(uint))] public uint Y;

    [FieldOffset(sizeof(uint) * 2)] public uint Z;

    [FieldOffset(0)] private Vector128<uint> value;

    public static Vector3Uint Zero => default;

    public static Vector3Uint One => new(1);

    public static Vector3Uint UnitX => new(1, 0, 0);
    public static Vector3Uint UnitY => new(0, 1, 0);
    public static Vector3Uint UnitZ => new(0, 0, 1);

    private static readonly unsafe int size = sizeof(Vector3Uint);

    public Vector3Uint(ReadOnlySpan<uint> values)
    {
        if (values.Length < 3)
        {
            throw new ArgumentException($"{nameof(values)} must be at least 3 elements in length");
        }

        this = Unsafe.ReadUnaligned<Vector3Uint>(ref Unsafe.As<uint, byte>(ref MemoryMarshal.GetReference(values)));
    }

    public Vector3Uint(uint value) : this(stackalloc[] { value, value, value })
    {
    }

    public Vector3Uint(uint x, uint y, uint z) : this(stackalloc[] { x, y, z })
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3Uint operator +(Vector3Uint left, Vector3Uint right)
    {
        var vec = left.value + right.value;
        return Unsafe.ReadUnaligned<Vector3Uint>(ref Unsafe.As<Vector128<uint>, byte>(ref vec));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3Uint operator -(Vector3Uint left, Vector3Uint right)
    {
        var vec = left.value - right.value;
        return Unsafe.ReadUnaligned<Vector3Uint>(ref Unsafe.As<Vector128<uint>, byte>(ref vec));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3Uint operator *(Vector3Uint left, Vector3Uint right)
    {
        var vec = left.value * right.value;
        return Unsafe.ReadUnaligned<Vector3Uint>(ref Unsafe.As<Vector128<uint>, byte>(ref vec));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3Uint operator /(Vector3Uint left, Vector3Uint right)
    {
        var vec = Vector128.Create(left.X / right.X, left.Y / right.Y, left.Z / right.Z, 0);
        return Unsafe.ReadUnaligned<Vector3Uint>(ref Unsafe.As<Vector128<uint>, byte>(ref vec));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3Uint operator *(Vector3Uint left, uint right)
    {
        var vec = left.value * right;
        return Unsafe.ReadUnaligned<Vector3Uint>(ref Unsafe.As<Vector128<uint>, byte>(ref vec));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3Uint operator *(uint left, Vector3Uint right)
    {
        var vec = right.value * left;
        return Unsafe.ReadUnaligned<Vector3Uint>(ref Unsafe.As<Vector128<uint>, byte>(ref vec));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3Uint operator -(Vector3Uint vector)
    {
        var vec = -vector.value;
        return Unsafe.ReadUnaligned<Vector3Uint>(ref Unsafe.As<Vector128<uint>, byte>(ref vec));
    }

    public bool Equals(Vector3Uint other)
    {
        return value.Equals(other.value);
    }

    public override bool Equals(object? obj)
    {
        return obj is Vector3Uint other && Equals(other);
    }

    public override int GetHashCode()
    {
        return value.GetHashCode();
    }

    public static bool operator ==(Vector3Uint left, Vector3Uint right)
    {
        return left.value == right.value;
    }

    public static bool operator !=(Vector3Uint left, Vector3Uint right)
    {
        return left.value != right.value;
    }

    public override string ToString()
    {
        return $"[{X}, {Y}, {Z}]";
    }
}
