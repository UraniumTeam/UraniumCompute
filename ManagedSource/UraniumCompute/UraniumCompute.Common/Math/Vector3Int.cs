using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace UraniumCompute.Common.Math;

[DeviceType("int3")]
[StructLayout(LayoutKind.Explicit)]
public struct Vector3Int : IEquatable<Vector3Int>
{
    public static Vector3Int Zero => default;

    public static Vector3Int One => new(1);

    public static Vector3Int UnitX => new(1, 0, 0);
    public static Vector3Int UnitY => new(0, 1, 0);
    public static Vector3Int UnitZ => new(0, 0, 1);
    [FieldOffset(0)] public int X;

    [FieldOffset(sizeof(int))] public int Y;

    [FieldOffset(sizeof(int) * 2)] public int Z;

    private static readonly unsafe int size = sizeof(Vector3Int);

    public int this[int index]
    {
        get
        {
            if ((uint)index >= 3)
            {
                throw new ArgumentOutOfRangeException();
            }

            return Unsafe.Add(ref Unsafe.As<Vector3Int, int>(ref this), index);
        }
        set
        {
            if ((uint)index >= 3)
            {
                throw new ArgumentOutOfRangeException();
            }

            Unsafe.Add(ref Unsafe.As<Vector3Int, int>(ref this), index) = value;
        }
    }

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
        return new Vector3Int(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3Int operator -(Vector3Int left, Vector3Int right)
    {
        return new Vector3Int(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3Int operator *(Vector3Int left, Vector3Int right)
    {
        return new Vector3Int(left.X * right.X, left.Y * right.Y, left.Z * right.Z);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3Int operator /(Vector3Int left, Vector3Int right)
    {
        return new Vector3Int(left.X / right.X, left.Y / right.Y, left.Z / right.Z);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3Int operator /(Vector3Int left, int right)
    {
        return new Vector3Int(left.X / right, left.Y / right, left.Z / right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3Int operator *(Vector3Int left, int right)
    {
        return new Vector3Int(left.X * right, left.Y * right, left.Z * right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3Int operator *(int left, Vector3Int right)
    {
        return new Vector3Int(right.X * left, right.Y * left, right.Z * left);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3Int operator -(Vector3Int vector)
    {
        return new Vector3Int(-vector.X, -vector.Y, -vector.Z);
    }

    public bool Equals(Vector3Int other)
    {
        return X.Equals(other.X) &&
               Y.Equals(other.Y) &&
               Z.Equals(other.Z);
    }

    public override bool Equals(object? obj)
    {
        return obj is Vector3Int other && Equals(other);
    }

    public override int GetHashCode()
    {
        HashCode hash = default;

        hash.Add(X);
        hash.Add(Y);
        hash.Add(Z);

        return hash.ToHashCode();
    }

    public static bool operator ==(Vector3Int left, Vector3Int right)
    {
        return left.X == right.X &&
               left.Y == right.Y &&
               left.Z == right.Z;
    }

    public static bool operator !=(Vector3Int left, Vector3Int right)
    {
        return left.X != right.X ||
               left.Y != right.Y ||
               left.Z != right.Z;
    }

    public override string ToString()
    {
        return $"[{X}, {Y}, {Z}]";
    }
}
