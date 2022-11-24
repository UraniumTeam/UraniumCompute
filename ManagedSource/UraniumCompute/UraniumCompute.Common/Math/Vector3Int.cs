using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace UraniumCompute.Common.Math;

[DeviceType("int3")]
[StructLayout(LayoutKind.Explicit)]
public struct Vector3Int : IEquatable<Vector3Int>
{
    [FieldOffset(0)] public int X;

    [FieldOffset(sizeof(int))] public int Y;

    [FieldOffset(sizeof(int) * 2)] public int Z;

    [FieldOffset(0)] private Vector128<int> value;

    public static Vector3Int Zero => default;

    public static Vector3Int One => new(1);

    public static Vector3Int UnitX => new(1, 0, 0);
    public static Vector3Int UnitY => new(0, 1, 0);
    public static Vector3Int UnitZ => new(0, 0, 1);

    private static readonly unsafe int size = sizeof(Vector3Int);

    public Vector3Int(ReadOnlySpan<int> values)
    {
        if (values.Length < 3)
        {
            throw new ArgumentException($"{nameof(values)} must be at least 3 elements in length");
        }

        this = Unsafe.ReadUnaligned<Vector3Int>(ref Unsafe.As<int, byte>(ref MemoryMarshal.GetReference(values)));
    }

    public Vector3Int(int value) : this(stackalloc[] { value, value, value })
    {
    }

    public Vector3Int(int x, int y, int z) : this(stackalloc[] { x, y, z })
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3Int operator +(Vector3Int left, Vector3Int right)
    {
        var vec = left.value + right.value;
        return Unsafe.ReadUnaligned<Vector3Int>(ref Unsafe.As<Vector128<int>, byte>(ref vec));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3Int operator -(Vector3Int left, Vector3Int right)
    {
        var vec = left.value - right.value;
        return Unsafe.ReadUnaligned<Vector3Int>(ref Unsafe.As<Vector128<int>, byte>(ref vec));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3Int operator *(Vector3Int left, Vector3Int right)
    {
        var vec = left.value * right.value;
        return Unsafe.ReadUnaligned<Vector3Int>(ref Unsafe.As<Vector128<int>, byte>(ref vec));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3Int operator /(Vector3Int left, Vector3Int right)
    {
        var vec = Vector128.Create(left.X / right.X, left.Y / right.Y, left.Z / right.Z, 0);
        return Unsafe.ReadUnaligned<Vector3Int>(ref Unsafe.As<Vector128<int>, byte>(ref vec));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3Int operator *(Vector3Int left, int right)
    {
        var vec = left.value * right;
        return Unsafe.ReadUnaligned<Vector3Int>(ref Unsafe.As<Vector128<int>, byte>(ref vec));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3Int operator *(int left, Vector3Int right)
    {
        var vec = right.value * left;
        return Unsafe.ReadUnaligned<Vector3Int>(ref Unsafe.As<Vector128<int>, byte>(ref vec));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3Int operator -(Vector3Int vector)
    {
        var vec = -vector.value;
        return Unsafe.ReadUnaligned<Vector3Int>(ref Unsafe.As<Vector128<int>, byte>(ref vec));
    }

    public bool Equals(Vector3Int other)
    {
        return value.Equals(other.value);
    }

    public override bool Equals(object? obj)
    {
        return obj is Vector3Int other && Equals(other);
    }

    public override int GetHashCode()
    {
        return value.GetHashCode();
    }

    public static bool operator ==(Vector3Int left, Vector3Int right)
    {
        return left.value == right.value;
    }

    public static bool operator !=(Vector3Int left, Vector3Int right)
    {
        return left.value != right.value;
    }

    public override string ToString()
    {
        return $"[{X}, {Y}, {Z}]";
    }
}
