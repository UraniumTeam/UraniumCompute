using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace UraniumCompute.Common.Math;

[DeviceType("uint3")]
[StructLayout(LayoutKind.Explicit)]
public struct Vector3Uint : IEquatable<Vector3Uint>
{
    public static Vector3Uint Zero => default;

    public static Vector3Uint One => new(1);

    public static Vector3Uint UnitX => new(1, 0, 0);
    public static Vector3Uint UnitY => new(0, 1, 0);
    public static Vector3Uint UnitZ => new(0, 0, 1);
    [FieldOffset(0)] public uint X;

    [FieldOffset(sizeof(uint))] public uint Y;

    [FieldOffset(sizeof(uint) * 2)] public uint Z;

    private static readonly unsafe int size = sizeof(Vector3Uint);

    public uint this[int index]
    {
        get
        {
            if ((uint)index >= 3)
            {
                throw new ArgumentOutOfRangeException();
            }

            return Unsafe.Add(ref Unsafe.As<Vector3Uint, uint>(ref this), index);
        }
        set
        {
            if ((uint)index >= 3)
            {
                throw new ArgumentOutOfRangeException();
            }

            Unsafe.Add(ref Unsafe.As<Vector3Uint, uint>(ref this), index) = value;
        }
    }

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
        return new Vector3Uint(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3Uint operator -(Vector3Uint left, Vector3Uint right)
    {
        return new Vector3Uint(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3Uint operator *(Vector3Uint left, Vector3Uint right)
    {
        return new Vector3Uint(left.X * right.X, left.Y * right.Y, left.Z * right.Z);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3Uint operator /(Vector3Uint left, Vector3Uint right)
    {
        return new Vector3Uint(left.X / right.X, left.Y / right.Y, left.Z / right.Z);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3Uint operator *(Vector3Uint left, uint right)
    {
        return new Vector3Uint(left.X * right, left.Y * right, left.Z * right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3Uint operator *(uint left, Vector3Uint right)
    {
        return new Vector3Uint(right.X * left, right.Y * left, right.Z * left);
    }

    public bool Equals(Vector3Uint other)
    {
        return X.Equals(other.X) &&
               Y.Equals(other.Y) &&
               Z.Equals(other.Z);
    }

    public override bool Equals(object? obj)
    {
        return obj is Vector3Uint other && Equals(other);
    }

    public override int GetHashCode()
    {
        HashCode hash = default;

        hash.Add(X);
        hash.Add(Y);
        hash.Add(Z);

        return hash.ToHashCode();
    }

    public static bool operator ==(Vector3Uint left, Vector3Uint right)
    {
        return left.X == right.X &&
               left.Y == right.Y &&
               left.Z == right.Z;
    }

    public static bool operator !=(Vector3Uint left, Vector3Uint right)
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
