using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace UraniumCompute.Common.Math;

[StructLayout(LayoutKind.Explicit)]
public struct Matrix3x3Int : IEquatable<Matrix3x3Int>
{
    [FieldOffset(0)] private Vector3Int row1;
    [FieldOffset(0)] public int M11;
    [FieldOffset(sizeof(int))] public int M12;
    [FieldOffset(sizeof(int) * 2)] public int M13;

    [FieldOffset(sizeof(int) * 3)] private Vector3Int row2;
    [FieldOffset(sizeof(int) * 3)] public int M21;
    [FieldOffset(sizeof(int) * 4)] public int M22;
    [FieldOffset(sizeof(int) * 5)] public int M23;

    [FieldOffset(sizeof(int) * 6)] private Vector3Int row3;
    [FieldOffset(sizeof(int) * 6)] public int M31;
    [FieldOffset(sizeof(int) * 7)] public int M32;
    [FieldOffset(sizeof(int) * 8)] public int M33;

    public Matrix3x3Int(
        int m11, int m12, int m13,
        int m21, int m22, int m23,
        int m31, int m32, int m33)
    {
        M11 = m11;
        M12 = m12;
        M13 = m13;

        M21 = m21;
        M22 = m22;
        M23 = m23;

        M31 = m31;
        M32 = m32;
        M33 = m33;
    }

    public Matrix3x3Int(ReadOnlySpan<int> values)
    {
        if (values.Length < 9)
        {
            throw new ArgumentException($"{nameof(values)} must be at least 9 elements in length");
        }

        this = Unsafe.ReadUnaligned<Matrix3x3Int>(ref Unsafe.As<int, byte>(ref MemoryMarshal.GetReference(values)));
    }

    public static Matrix3x3Int Identity { get; } = new
    (
        1, 0, 0,
        0, 1, 0,
        0, 0, 1
    );

    public int this[int row, int column]
    {
        get
        {
            if ((uint)row >= 3)
                throw new ArgumentOutOfRangeException();

            var vRow = Unsafe.Add(ref Unsafe.As<int, Vector3Int>(ref M11), row);
            return vRow[column];
        }
        set
        {
            if ((uint)row >= 3)
                throw new ArgumentOutOfRangeException();

            ref var vRow = ref Unsafe.Add(ref Unsafe.As<int, Vector3Int>(ref M11), row);
            vRow[column] = value;
        }
    }

    public readonly bool IsIdentity =>
        M11 == 1 && M22 == 1 && M33 == 1 &&
        M12 == 0 && M13 == 0 &&
        M21 == 0 && M23 == 0 &&
        M31 == 0 && M32 == 0;

    public static Matrix3x3Int operator +(Matrix3x3Int left, Matrix3x3Int right)
    {
        left.row1 += right.row1;
        left.row2 += right.row2;
        left.row3 += right.row3;
        return left;
    }

    public static Matrix3x3Int operator -(Matrix3x3Int left, Matrix3x3Int right)
    {
        left.row1 -= right.row1;
        left.row2 -= right.row2;
        left.row3 -= right.row3;
        return left;
    }

    public static Matrix3x3Int operator -(Matrix3x3Int value)
    {
        value.row1 = -value.row1;
        value.row2 = -value.row2;
        value.row3 = -value.row3;

        return value;
    }

    public static Matrix3x3Int operator *(Matrix3x3Int left, Matrix3x3Int right)
    {
        return new Matrix3x3Int(
            left.M11 * right.M11 + left.M12 * right.M21 + left.M13 * right.M31,
            left.M11 * right.M12 + left.M12 * right.M22 + left.M13 * right.M32,
            left.M11 * right.M13 + left.M12 * right.M23 + left.M13 * right.M33,
            left.M21 * right.M11 + left.M22 * right.M21 + left.M23 * right.M31,
            left.M21 * right.M12 + left.M22 * right.M22 + left.M23 * right.M32,
            left.M21 * right.M13 + left.M22 * right.M23 + left.M23 * right.M33,
            left.M31 * right.M11 + left.M32 * right.M21 + left.M33 * right.M31,
            left.M31 * right.M12 + left.M32 * right.M22 + left.M33 * right.M32,
            left.M31 * right.M13 + left.M32 * right.M23 + left.M33 * right.M33);
    }

    public static Matrix3x3Int operator *(Matrix3x3Int matrix, int scalar)
    {
        matrix.row1 *= scalar;
        matrix.row2 *= scalar;
        matrix.row3 *= scalar;
        return matrix;
    }

    public static Matrix3x3Int operator *(int scalar, Matrix3x3Int matrix)
    {
        return matrix * scalar;
    }

    public static bool operator ==(Matrix3x3Int left, Matrix3x3Int right)
    {
        return left.row1 == right.row1 &&
               left.row2 == right.row2 &&
               left.row3 == right.row3;
    }

    public static bool operator !=(Matrix3x3Int left, Matrix3x3Int right)
    {
        return left.row1 != right.row1 ||
               left.row2 != right.row2 ||
               left.row3 != right.row3;
    }


    public static Matrix3x3Int Transpose(Matrix3x3Int matrix)
    {
        return new Matrix3x3Int(
            matrix.M11, matrix.M21, matrix.M31,
            matrix.M12, matrix.M22, matrix.M32,
            matrix.M13, matrix.M23, matrix.M33);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly override bool Equals(object? obj)
    {
        return obj is Matrix3x3Int other && Equals(other);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Equals(Matrix3x3Int other)
    {
        return row1.Equals(other.row1) &&
               row2.Equals(other.row2) &&
               row3.Equals(other.row3);
    }

    public readonly int GetDeterminant()
        => M11 * M22 * M33 +
           M12 * M23 * M31 +
           M13 * M21 * M32 -
           M13 * M22 * M31 -
           M11 * M23 * M32 -
           M12 * M21 * M33;

    public readonly override int GetHashCode()
    {
        HashCode hash = default;

        hash.Add(row1);
        hash.Add(row2);
        hash.Add(row3);

        return hash.ToHashCode();
    }

    public readonly override string ToString() =>
        $"{{ {{M11:{M11} M12:{M12} M13:{M13}}} {{M21:{M21} M22:{M22} M23:{M23}}} {{M31:{M31} M32:{M32} M33:{M33}}} }}";
}
