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
            if ((uint)row >= 1)
                throw new ArgumentOutOfRangeException();

            var vRow = Unsafe.Add(ref Unsafe.As<float, Vector2>(ref M11), row);
            return vRow[column];
        }
        set
        {
            if ((uint)row >= 1)
                throw new ArgumentOutOfRangeException();

            ref var vRow = ref Unsafe.Add(ref Unsafe.As<float, Vector2>(ref M11), row);
            vRow[1] = value;
        }
    }

    public readonly bool IsIdentity => M11 == 1f && M22 == 1f && M12 == 0f && M21 == 0f;

    public static unsafe Matrix2x2 operator +(Matrix2x2 value1, Matrix2x2 value2)
    {
        if (AdvSimd.IsSupported)
        {
            AdvSimd.Store(&value1.M11,
                AdvSimd.Add(AdvSimd.LoadVector64(&value1.M11), AdvSimd.LoadVector64(&value2.M11)));
            AdvSimd.Store(&value1.M21,
                AdvSimd.Add(AdvSimd.LoadVector64(&value1.M21), AdvSimd.LoadVector64(&value2.M21)));
            return value1;
        }

        if (Sse.IsSupported)
        {
            Sse.Store(&value1.M11, Sse.Add(Sse.LoadVector128(&value1.M11), Sse.LoadVector128(&value2.M11)));
            return value1;
        }

        var vec = Vector128.Create(value1.row1 + value2.row1, value1.row2 + value2.row2);
        return Unsafe.ReadUnaligned<Matrix2x2>(ref Unsafe.As<Vector128<float>, byte>(ref vec));
        // or
        // Vector64<float>[] vec = { value1.row1 + value2.row1, value1.row2 + value2.row2 };
        // return Unsafe.ReadUnaligned<Matrix2x2>(ref Unsafe.As<Vector64<float>[], byte>(ref vec));
        // or
        // return new Matrix2x2(value1.row1 + value2.row1, value1.row2 + value2.row2);
    }

    public static unsafe Matrix2x2 operator -(Matrix2x2 value1, Matrix2x2 value2)
    {
        if (AdvSimd.IsSupported)
        {
            AdvSimd.Store(&value1.M11,
                AdvSimd.Subtract(AdvSimd.LoadVector64(&value1.M11), AdvSimd.LoadVector64(&value2.M11)));
            AdvSimd.Store(&value1.M21,
                AdvSimd.Subtract(AdvSimd.LoadVector64(&value1.M21), AdvSimd.LoadVector64(&value2.M21)));
            return value1;
        }

        if (Sse.IsSupported)
        {
            Sse.Store(&value1.M11, Sse.Subtract(Sse.LoadVector128(&value1.M11), Sse.LoadVector128(&value2.M11)));
            return value1;
        }

        var vec = Vector128.Create(value1.row1 - value2.row1, value1.row2 - value2.row2);
        return Unsafe.ReadUnaligned<Matrix2x2>(ref Unsafe.As<Vector128<float>, byte>(ref vec));
        // or
        // Vector64<float>[] vec = { value1.row1 - value2.row1, value1.row2 - value2.row2 };
        // return Unsafe.ReadUnaligned<Matrix2x2>(ref Unsafe.As<Vector64<float>[], byte>(ref vec));
        // or
        // return new Matrix2x2(value1.row1 - value2.row1, value1.row2 - value2.row2);
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
        // or
        // return new Matrix2x2(-value.M11, -value.M12, -value.M21, -value.M21);
    }

    public static unsafe Matrix2x2 operator *(Matrix2x2 value1, Matrix2x2 value2)
    {
        if (AdvSimd.Arm64.IsSupported)
        {
            Unsafe.SkipInit(out Matrix2x2 result);

            Vector64<float> value1Row1 = AdvSimd.LoadVector64(&value1.M11);

            Vector64<float> vX = AdvSimd.MultiplyBySelectedScalar(AdvSimd.LoadVector64(&value2.M11), value1Row1, 0);
            Vector64<float> vY = AdvSimd.MultiplyBySelectedScalar(AdvSimd.LoadVector64(&value2.M21), value1Row1, 1);

            AdvSimd.Store(&result.M11, AdvSimd.Add(vX, vY));

            Vector64<float> value1Row2 = AdvSimd.LoadVector64(&value1.M21);

            vX = AdvSimd.MultiplyBySelectedScalar(AdvSimd.LoadVector64(&value2.M11), value1Row2, 0);
            vY = AdvSimd.MultiplyBySelectedScalar(AdvSimd.LoadVector64(&value2.M21), value1Row2, 1);

            AdvSimd.Store(&result.M21, AdvSimd.Add(vX, vY));

            return result;
        }

        if (Sse.IsSupported)
        {
            Vector128<float> zero = Vector128<float>.Zero;
            Vector128<float> m1 = Sse.LoadVector128(&value1.M11);
            Vector128<float> m2 = Sse.LoadVector128(&value2.M11);
            Vector128<float> m2ShiftLeft = Sse.Shuffle(m2, m2, 0x93);
            Sse.Store(
                &value1.M11,
                Sse.Add(
                    Sse.Add(Sse.Multiply(Sse.Shuffle(m1, zero, 0x00), m2),
                        Sse.Multiply(Sse.Shuffle(m1, zero, 0x55), Sse.Shuffle(m2ShiftLeft, m2ShiftLeft, 0x93))),
                    Sse.Add(Sse.Multiply(Sse.Shuffle(zero, m1, 0xAA), Sse.Shuffle(m2ShiftLeft, m2ShiftLeft, 0x93)),
                        Sse.Multiply(Sse.Shuffle(zero, m1, 0xFF), m2))));
            return value1;
        }

        var vec = Vector128.Create(
            value2.row1 * value1.M11 + value2.row2 * value1.M12,
            value2.row1 * value1.M21 + value2.row2 * value1.M22);
        return Unsafe.ReadUnaligned<Matrix2x2>(ref Unsafe.As<Vector128<float>, byte>(ref vec));
        // or
        // return new Matrix2x2(
        //     value1.M11 * value2.M11 + value1.M12 * value2.M21,
        //     value1.M11 * value2.M12 + value1.M12 * value2.M22,
        //     value1.M21 * value2.M11 + value1.M22 * value2.M21,
        //     value1.M21 * value2.M12 + value1.M22 * value2.M22
        // );
    }

    public static unsafe Matrix2x2 operator *(Matrix2x2 matrix, float scalar)
    {
        if (AdvSimd.IsSupported)
        {
            var value2Vec = Vector64.Create(scalar);
            AdvSimd.Store(&matrix.M11, AdvSimd.Multiply(AdvSimd.LoadVector64(&matrix.M11), value2Vec));
            AdvSimd.Store(&matrix.M21, AdvSimd.Multiply(AdvSimd.LoadVector64(&matrix.M21), value2Vec));
            return matrix;
        }
        else if (Sse.IsSupported)
        {
            var value2Vec = Vector128.Create(scalar);
            Sse.Store(&matrix.M11, Sse.Multiply(Sse.LoadVector128(&matrix.M11), value2Vec));
            return matrix;
        }

        var vec = Vector128.Create(matrix.row1 * scalar, matrix.row2 * scalar);
        return Unsafe.ReadUnaligned<Matrix2x2>(ref Unsafe.As<Vector128<float>, byte>(ref vec));
        // or
        // return new Matrix2x2(value1.M11 * value2, value1.M12 * value2, value1.M21 * value2, value1.M21 * value2);
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
        else if (Sse.IsSupported)
        {
            var v = Vector128.Create(scalar);
            Sse.Store(&matrix.M11, Sse.Multiply(Sse.LoadVector128(&matrix.M11), v));
            return matrix;
        }

        var vec = Vector128.Create(matrix.row1 * scalar, matrix.row2 * scalar);
        return Unsafe.ReadUnaligned<Matrix2x2>(ref Unsafe.As<Vector128<float>, byte>(ref vec));
        // or
        // return new Matrix2x2(value1.M11 * value2, value1.M12 * value2, value1.M21 * value2, value1.M21 * value2);
    }

    public static bool operator ==(Matrix2x2 value1, Matrix2x2 value2)
    {
        return value1.row1 == value2.row1 &&
               value1.row2 == value2.row2;
    }

    public static bool operator !=(Matrix2x2 value1, Matrix2x2 value2)
    {
        return value1.row1 != value2.row1 ||
               value1.row2 != value2.row2;
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
        if (Vector64.IsHardwareAccelerated)
        {
            return Vector64.LoadUnsafe(ref Unsafe.AsRef(in M11)).Equals(Vector64.LoadUnsafe(ref other.M11))
                   && Vector64.LoadUnsafe(ref Unsafe.AsRef(in M21)).Equals(Vector64.LoadUnsafe(ref other.M21));
        }

        return row1.Equals(other.row1) &&
               row2.Equals(other.row2);
    }

    public readonly float GetDeterminant() => M11 * M22 - M12 * M21;

    public readonly override int GetHashCode()
    {
        HashCode hash = default;

        hash.Add(M11);
        hash.Add(M12);
        hash.Add(M21);
        hash.Add(M22);

        return hash.ToHashCode();
    }

    public readonly override string ToString() =>
        $"{{ {{M11:{M11} M12:{M12} }} {{M21:{M21} M22:{M22} }} }}";
}
