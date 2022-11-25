using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace UraniumCompute.Common.Math;

[DeviceType("int4")]
[StructLayout(LayoutKind.Explicit)]
public struct Vector4Int : IEquatable<Vector4Int>
{
    [FieldOffset(0)] public int X;

    [FieldOffset(sizeof(int))] public int Y;

    [FieldOffset(sizeof(int) * 2)] public int Z;

    [FieldOffset(sizeof(int) * 3)] public int W;

    [FieldOffset(0)] private Vector128<int> value;

    public static Vector4Int Zero => default;

    public static Vector4Int One => new(1);

    public static Vector4Int UnitX => new(1, 0, 0, 0);
    public static Vector4Int UnitY => new(0, 1, 0, 0);
    public static Vector4Int UnitZ => new(0, 0, 1, 0);
    public static Vector4Int UnitW => new(0, 0, 0, 1);

    private static readonly unsafe int size = sizeof(Vector4Int);

    public Vector4Int(ReadOnlySpan<int> values)
    {
        if (values.Length < 4)
        {
            throw new ArgumentException($"{nameof(values)} must be at least 4 elements in length");
        }

        this = Unsafe.ReadUnaligned<Vector4Int>(ref Unsafe.As<int, byte>(ref MemoryMarshal.GetReference(values)));
    }

    public Vector4Int(int value) : this(stackalloc[] { value, value, value, value })
    {
    }

    public Vector4Int(int x, int y, int z, int w) : this(stackalloc[] { x, y, z, w })
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4Int operator +(Vector4Int left, Vector4Int right)
    {
        var vec = left.value + right.value;
        return Unsafe.ReadUnaligned<Vector4Int>(ref Unsafe.As<Vector128<int>, byte>(ref vec));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4Int operator -(Vector4Int left, Vector4Int right)
    {
        var vec = left.value - right.value;
        return Unsafe.ReadUnaligned<Vector4Int>(ref Unsafe.As<Vector128<int>, byte>(ref vec));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4Int operator *(Vector4Int left, Vector4Int right)
    {
        var vec = left.value * right.value;
        return Unsafe.ReadUnaligned<Vector4Int>(ref Unsafe.As<Vector128<int>, byte>(ref vec));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4Int operator /(Vector4Int left, Vector4Int right)
    {
        var vec = left.value / right.value;
        return Unsafe.ReadUnaligned<Vector4Int>(ref Unsafe.As<Vector128<int>, byte>(ref vec));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4Int operator *(Vector4Int left, int right)
    {
        var vec = left.value * right;
        return Unsafe.ReadUnaligned<Vector4Int>(ref Unsafe.As<Vector128<int>, byte>(ref vec));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4Int operator *(int left, Vector4Int right)
    {
        var vec = right.value * left;
        return Unsafe.ReadUnaligned<Vector4Int>(ref Unsafe.As<Vector128<int>, byte>(ref vec));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4Int operator -(Vector4Int vector)
    {
        var vec = -vector.value;
        return Unsafe.ReadUnaligned<Vector4Int>(ref Unsafe.As<Vector128<int>, byte>(ref vec));
    }

    public bool Equals(Vector4Int other)
    {
        return value.Equals(other.value);
    }

    public override bool Equals(object? obj)
    {
        return obj is Vector4Int other && Equals(other);
    }

    public override int GetHashCode()
    {
        return value.GetHashCode();
    }

    public static bool operator ==(Vector4Int left, Vector4Int right)
    {
        return left.value == right.value;
    }

    public static bool operator !=(Vector4Int left, Vector4Int right)
    {
        return left.value != right.value;
    }

    public override string ToString()
    {
        return $"[{X}, {Y}, {Z}, {W}]";
    }
}
