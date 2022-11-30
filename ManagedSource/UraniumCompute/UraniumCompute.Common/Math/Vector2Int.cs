using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace UraniumCompute.Common.Math;

[DeviceType("int2")]
[StructLayout(LayoutKind.Explicit)]
public struct Vector2Int : IEquatable<Vector2Int>
{
    [FieldOffset(0)] public int X;

    [FieldOffset(sizeof(int))] public int Y;

    [FieldOffset(0)] private Vector64<int> value;

    public static Vector2Int Zero => default;

    public static Vector2Int One => new(1);

    public static Vector2Int UnitX => new(1, 0);
    public static Vector2Int UnitY => new(0, 1);

    private static readonly unsafe int size = sizeof(Vector2Int);

    public int this[int index]
    {
        get
        {
            if ((uint)index >= 2)
            {
                throw new ArgumentOutOfRangeException();
            }

            return Unsafe.Add(ref Unsafe.As<Vector2Int, int>(ref this), index);
        }
        set
        {
            if ((uint)index >= 2)
            {
                throw new ArgumentOutOfRangeException();
            }

            Unsafe.Add(ref Unsafe.As<Vector2Int, int>(ref this), index) = value;
        }
    }

    public Vector2Int(ReadOnlySpan<int> values)
    {
        if (values.Length < 2)
        {
            throw new ArgumentException($"{nameof(values)} must be at least 2 elements in length");
        }

        this = Unsafe.ReadUnaligned<Vector2Int>(ref Unsafe.As<int, byte>(ref MemoryMarshal.GetReference(values)));
    }

    public Vector2Int(int value) : this(stackalloc[] { value, value })
    {
    }

    public Vector2Int(int x, int y) : this(stackalloc[] { x, y })
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2Int operator +(Vector2Int left, Vector2Int right)
    {
        var vec = left.value + right.value;
        return Unsafe.ReadUnaligned<Vector2Int>(ref Unsafe.As<Vector64<int>, byte>(ref vec));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2Int operator -(Vector2Int left, Vector2Int right)
    {
        var vec = left.value - right.value;
        return Unsafe.ReadUnaligned<Vector2Int>(ref Unsafe.As<Vector64<int>, byte>(ref vec));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2Int operator *(Vector2Int left, Vector2Int right)
    {
        var vec = left.value * right.value;
        return Unsafe.ReadUnaligned<Vector2Int>(ref Unsafe.As<Vector64<int>, byte>(ref vec));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2Int operator /(Vector2Int left, Vector2Int right)
    {
        var vec = left.value / right.value;
        return Unsafe.ReadUnaligned<Vector2Int>(ref Unsafe.As<Vector64<int>, byte>(ref vec));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2Int operator *(Vector2Int left, int right)
    {
        var vec = left.value * right;
        return Unsafe.ReadUnaligned<Vector2Int>(ref Unsafe.As<Vector64<int>, byte>(ref vec));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2Int operator *(int left, Vector2Int right)
    {
        var vec = right.value * left;
        return Unsafe.ReadUnaligned<Vector2Int>(ref Unsafe.As<Vector64<int>, byte>(ref vec));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2Int operator -(Vector2Int vector)
    {
        var vec = -vector.value;
        return Unsafe.ReadUnaligned<Vector2Int>(ref Unsafe.As<Vector64<int>, byte>(ref vec));
    }

    public bool Equals(Vector2Int other)
    {
        return value.Equals(other.value);
    }

    public override bool Equals(object? obj)
    {
        return obj is Vector2Int other && Equals(other);
    }

    public override int GetHashCode()
    {
        return value.GetHashCode();
    }

    public static bool operator ==(Vector2Int left, Vector2Int right)
    {
        return left.value == right.value;
    }

    public static bool operator !=(Vector2Int left, Vector2Int right)
    {
        return left.value != right.value;
    }

    public override string ToString()
    {
        return $"[{X}, {Y}]";
    }
}
