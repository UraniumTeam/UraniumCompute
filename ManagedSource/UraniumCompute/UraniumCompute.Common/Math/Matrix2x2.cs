using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;

namespace UraniumCompute.Common.Math;

[StructLayout(LayoutKind.Explicit)]
public struct Matrix2x2 : IEquatable<Matrix2x2>
{
    [FieldOffset(0)] private Vector64<float> row1;
    [FieldOffset(0)] public float M11;
    [FieldOffset(sizeof(float))] public float M12;

    [FieldOffset(sizeof(float) * 2)] private Vector64<float> row2;
    [FieldOffset(sizeof(float) * 2)] public float M21;
    [FieldOffset(sizeof(float) * 3)] public float M22;

    public Matrix2x2(float m11, float m12, float m21, float m22)
    {
        M11 = m11;
        M12 = m12;
        M21 = m21;
        M22 = m22;
    }

    public Matrix2x2(ReadOnlySpan<float> values)
    {
        if (values.Length < 4)
        {
            throw new ArgumentException($"{nameof(values)} must be at least 4 elements in length");
        }

        this = Unsafe.ReadUnaligned<Matrix2x2>(ref Unsafe.As<float, byte>(ref MemoryMarshal.GetReference(values)));
    }

    private Matrix2x2(Vector64<float> firstRow, Vector64<float> secondRow)
    {
        row1 = firstRow;
        row2 = secondRow;
    }

    public static Matrix2x2 Identity { get; } = new
    (
        1f, 0f,
        0f, 1f
    );

    public float this[int row, int column]
    {
        get
        {
            if ((uint)row >= 2)
                throw new ArgumentOutOfRangeException();

            var vRow = Unsafe.Add(ref Unsafe.As<float, Vector2>(ref M11), row);
            return vRow[column];
        }
        set
        {
            if ((uint)row >= 2)
                throw new ArgumentOutOfRangeException();

            ref var vRow = ref Unsafe.Add(ref Unsafe.As<float, Vector2>(ref M11), row);
            vRow[column] = value;
        }
    }

    public readonly bool IsIdentity => M11 == 1f && M22 == 1f && M12 == 0f && M21 == 0f;

    public static unsafe Matrix2x2 operator +(Matrix2x2 left, Matrix2x2 right)
    {
        if (AdvSimd.IsSupported)
        {
            AdvSimd.Store(&left.M11,
                AdvSimd.Add(AdvSimd.LoadVector64(&left.M11), AdvSimd.LoadVector64(&right.M11)));
            AdvSimd.Store(&left.M21,
                AdvSimd.Add(AdvSimd.LoadVector64(&left.M21), AdvSimd.LoadVector64(&right.M21)));
            return left;
        }

        if (Sse.IsSupported)
        {
            Sse.Store(&left.M11, Sse.Add(Sse.LoadVector128(&left.M11), Sse.LoadVector128(&right.M11)));
            return left;
        }

        var vec = Vector128.Create(left.row1 + right.row1, left.row2 + right.row2);
        return Unsafe.ReadUnaligned<Matrix2x2>(ref Unsafe.As<Vector128<float>, byte>(ref vec));
    }

    public static unsafe Matrix2x2 operator -(Matrix2x2 left, Matrix2x2 right)
    {
        if (AdvSimd.IsSupported)
        {
            AdvSimd.Store(&left.M11,
                AdvSimd.Subtract(AdvSimd.LoadVector64(&left.M11), AdvSimd.LoadVector64(&right.M11)));
            AdvSimd.Store(&left.M21,
                AdvSimd.Subtract(AdvSimd.LoadVector64(&left.M21), AdvSimd.LoadVector64(&right.M21)));
            return left;
        }

        if (Sse.IsSupported)
        {
            Sse.Store(&left.M11, Sse.Subtract(Sse.LoadVector128(&left.M11), Sse.LoadVector128(&right.M11)));
            return left;
        }

        var vec = Vector128.Create(left.row1 - right.row1, left.row2 - right.row2);
        return Unsafe.ReadUnaligned<Matrix2x2>(ref Unsafe.As<Vector128<float>, byte>(ref vec));
    }

    public static unsafe Matrix2x2 operator -(Matrix2x2 value)
    {
        if (AdvSimd.IsSupported)
        {
            AdvSimd.Store(&value.M11, AdvSimd.Negate(AdvSimd.LoadVector64(&value.M11)));
            AdvSimd.Store(&value.M21, AdvSimd.Negate(AdvSimd.LoadVector64(&value.M21)));
            return value;
        }

        if (Sse.IsSupported)
        {
            var zero = Vector128<float>.Zero;
            Sse.Store(&value.M11, Sse.Subtract(zero, Sse.LoadVector128(&value.M11)));
            return value;
        }

        var vec = Vector128.Create(-value.row1, -value.row2);
        return Unsafe.ReadUnaligned<Matrix2x2>(ref Unsafe.As<Vector128<float>, byte>(ref vec));
    }

    public static unsafe Matrix2x2 operator *(Matrix2x2 left, Matrix2x2 right)
    {
        if (AdvSimd.Arm64.IsSupported)
        {
            Unsafe.SkipInit(out Matrix2x2 result);

            var leftRow1 = AdvSimd.LoadVector64(&left.M11);

            var vX = AdvSimd.MultiplyBySelectedScalar(AdvSimd.LoadVector64(&right.M11), leftRow1, 0);
            var vY = AdvSimd.MultiplyBySelectedScalar(AdvSimd.LoadVector64(&right.M21), leftRow1, 1);

            AdvSimd.Store(&result.M11, AdvSimd.Add(vX, vY));

            var leftRow2 = AdvSimd.LoadVector64(&left.M21);

            vX = AdvSimd.MultiplyBySelectedScalar(AdvSimd.LoadVector64(&right.M11), leftRow2, 0);
            vY = AdvSimd.MultiplyBySelectedScalar(AdvSimd.LoadVector64(&right.M21), leftRow2, 1);

            AdvSimd.Store(&result.M21, AdvSimd.Add(vX, vY));

            return result;
        }

        if (Sse.IsSupported)
        {
            var zero = Vector128<float>.Zero;
            var l = Sse.LoadVector128(&left.M11);
            var r = Sse.LoadVector128(&right.M11);
            var m2ShiftLeft = Sse.Shuffle(r, r, 0x93);
            Sse.Store(
                &left.M11,
                Sse.Add(
                    Sse.Add(Sse.Multiply(Sse.Shuffle(l, zero, 0x00), r),
                        Sse.Multiply(Sse.Shuffle(l, zero, 0x55), Sse.Shuffle(m2ShiftLeft, m2ShiftLeft, 0x93))),
                    Sse.Add(Sse.Multiply(Sse.Shuffle(zero, l, 0xAA), Sse.Shuffle(m2ShiftLeft, m2ShiftLeft, 0x93)),
                        Sse.Multiply(Sse.Shuffle(zero, l, 0xFF), r))));
            return left;
        }

        var vec = Vector128.Create(
            right.row1 * left.M11 + right.row2 * left.M12,
            right.row1 * left.M21 + right.row2 * left.M22);
        return Unsafe.ReadUnaligned<Matrix2x2>(ref Unsafe.As<Vector128<float>, byte>(ref vec));
    }

    public static unsafe Matrix2x2 operator *(Matrix2x2 matrix, float scalar)
    {
        if (AdvSimd.IsSupported)
        {
            var v = Vector64.Create(scalar);
            AdvSimd.Store(&matrix.M11, AdvSimd.Multiply(AdvSimd.LoadVector64(&matrix.M11), v));
            AdvSimd.Store(&matrix.M21, AdvSimd.Multiply(AdvSimd.LoadVector64(&matrix.M21), v));
            return matrix;
        }

        if (Sse.IsSupported)
        {
            var v = Vector128.Create(scalar);
            Sse.Store(&matrix.M11, Sse.Multiply(Sse.LoadVector128(&matrix.M11), v));
            return matrix;
        }

        var vec = Vector128.Create(matrix.row1 * scalar, matrix.row2 * scalar);
        return Unsafe.ReadUnaligned<Matrix2x2>(ref Unsafe.As<Vector128<float>, byte>(ref vec));
    }


    public static unsafe Matrix2x2 operator *(float scalar, Matrix2x2 matrix)
    {
        if (AdvSimd.IsSupported)
        {
            var v = Vector64.Create(scalar);
            AdvSimd.Store(&matrix.M11, AdvSimd.Multiply(AdvSimd.LoadVector64(&matrix.M11), v));
            AdvSimd.Store(&matrix.M21, AdvSimd.Multiply(AdvSimd.LoadVector64(&matrix.M21), v));
            return matrix;
        }

        if (Sse.IsSupported)
        {
            var v = Vector128.Create(scalar);
            Sse.Store(&matrix.M11, Sse.Multiply(Sse.LoadVector128(&matrix.M11), v));
            return matrix;
        }

        var vec = Vector128.Create(matrix.row1 * scalar, matrix.row2 * scalar);
        return Unsafe.ReadUnaligned<Matrix2x2>(ref Unsafe.As<Vector128<float>, byte>(ref vec));
    }

    public static bool operator ==(Matrix2x2 left, Matrix2x2 right)
    {
        return left.row1 == right.row1 &&
               left.row2 == right.row2;
    }

    public static bool operator !=(Matrix2x2 left, Matrix2x2 right)
    {
        return left.row1 != right.row1 ||
               left.row2 != right.row2;
    }

    public static unsafe Matrix2x2 Transpose(Matrix2x2 matrix)
    {
        // if (AdvSimd.Arm64.IsSupported)
        // {
        //     
        // }
        // else if (Sse.IsSupported)
        // {
        //     
        // }

        return new Matrix2x2(matrix.M11, matrix.M21, matrix.M12, matrix.M22);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly override bool Equals(object? obj)
    {
        return obj is Matrix2x2 other && Equals(other);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Equals(Matrix2x2 other)
    {
        return row1.Equals(other.row1) &&
               row2.Equals(other.row2);
    }

    public readonly float GetDeterminant() => M11 * M22 - M12 * M21;

    public readonly override int GetHashCode()
    {
        HashCode hash = default;

        hash.Add(row1);
        hash.Add(row2);

        return hash.ToHashCode();
    }

    public readonly override string ToString() =>
        $"{{ {{M11:{M11} M12:{M12} }} {{M21:{M21} M22:{M22} }} }}";
}
