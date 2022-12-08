using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;

namespace UraniumCompute.Common.Math;

[StructLayout(LayoutKind.Explicit)]
public struct Matrix4x4Int : IEquatable<Matrix4x4Int>
{
    [FieldOffset(0)] private Vector128<int> row1;
    [FieldOffset(0)] public int M11;
    [FieldOffset(sizeof(int))] public int M12;
    [FieldOffset(sizeof(int) * 2)] public int M13;
    [FieldOffset(sizeof(int) * 3)] public int M14;

    [FieldOffset(sizeof(int) * 4)] private Vector128<int> row2;
    [FieldOffset(sizeof(int) * 4)] public int M21;
    [FieldOffset(sizeof(int) * 5)] public int M22;
    [FieldOffset(sizeof(int) * 6)] public int M23;
    [FieldOffset(sizeof(int) * 7)] public int M24;

    [FieldOffset(sizeof(int) * 8)] private Vector128<int> row3;
    [FieldOffset(sizeof(int) * 8)] public int M31;
    [FieldOffset(sizeof(int) * 9)] public int M32;
    [FieldOffset(sizeof(int) * 10)] public int M33;
    [FieldOffset(sizeof(int) * 11)] public int M34;

    [FieldOffset(sizeof(int) * 12)] private Vector128<int> row4;
    [FieldOffset(sizeof(int) * 12)] public int M41;
    [FieldOffset(sizeof(int) * 13)] public int M42;
    [FieldOffset(sizeof(int) * 14)] public int M43;
    [FieldOffset(sizeof(int) * 15)] public int M44;

    public Matrix4x4Int(
        int m11, int m12, int m13, int m14,
        int m21, int m22, int m23, int m24,
        int m31, int m32, int m33, int m34,
        int m41, int m42, int m43, int m44)
    {
        M11 = m11;
        M12 = m12;
        M13 = m13;
        M14 = m14;

        M21 = m21;
        M22 = m22;
        M23 = m23;
        M24 = m24;

        M31 = m31;
        M32 = m32;
        M33 = m33;
        M34 = m34;

        M41 = m41;
        M42 = m42;
        M43 = m43;
        M44 = m44;
    }

    public Matrix4x4Int(ReadOnlySpan<int> values)
    {
        if (values.Length < 16)
        {
            throw new ArgumentException($"{nameof(values)} must be at least 16 elements in length");
        }

        this = Unsafe.ReadUnaligned<Matrix4x4Int>(ref Unsafe.As<int, byte>(ref MemoryMarshal.GetReference(values)));
    }

    public static Matrix4x4Int Identity { get; } = new
    (
        1, 0, 0, 0,
        0, 1, 0, 0,
        0, 0, 1, 0,
        0, 0, 0, 1
    );

    public unsafe int this[int row, int column]
    {
        get
        {
            if ((uint)row >= 4)
                throw new ArgumentOutOfRangeException();

            var vRow = Unsafe.Add(ref Unsafe.As<int, Vector4Int>(ref M11), row);
            return vRow[column];
        }
        set
        {
            if ((uint)row >= 4)
                throw new ArgumentOutOfRangeException();

            ref var vRow = ref Unsafe.Add(ref Unsafe.As<int, Vector4Int>(ref M11), row);
            vRow[column] = value;
        }
    }

    public readonly bool IsIdentity =>
        M11 == 1 && M22 == 1 && M33 == 1 && M44 == 1 &&
        M12 == 0 && M13 == 0 && M14 == 0 &&
        M21 == 0 && M23 == 0 && M24 == 0 &&
        M31 == 0 && M32 == 0 && M34 == 0 &&
        M41 == 0 && M42 == 0 && M43 == 0;

    public static unsafe Matrix4x4Int operator +(Matrix4x4Int left, Matrix4x4Int right)
    {
        if (AdvSimd.IsSupported)
        {
            AdvSimd.Store(&left.M11,
                AdvSimd.Add(AdvSimd.LoadVector128(&left.M11), AdvSimd.LoadVector128(&right.M11)));
            AdvSimd.Store(&left.M21,
                AdvSimd.Add(AdvSimd.LoadVector128(&left.M21), AdvSimd.LoadVector128(&right.M21)));
            AdvSimd.Store(&left.M31,
                AdvSimd.Add(AdvSimd.LoadVector128(&left.M31), AdvSimd.LoadVector128(&right.M31)));
            AdvSimd.Store(&left.M41,
                AdvSimd.Add(AdvSimd.LoadVector128(&left.M41), AdvSimd.LoadVector128(&right.M41)));
            return left;
        }

        if (Sse2.IsSupported)
        {
            Sse2.Store(&left.M11, Sse2.Add(Sse2.LoadVector128(&left.M11), Sse2.LoadVector128(&right.M11)));
            Sse2.Store(&left.M21, Sse2.Add(Sse2.LoadVector128(&left.M21), Sse2.LoadVector128(&right.M21)));
            Sse2.Store(&left.M31, Sse2.Add(Sse2.LoadVector128(&left.M31), Sse2.LoadVector128(&right.M31)));
            Sse2.Store(&left.M41, Sse2.Add(Sse2.LoadVector128(&left.M41), Sse2.LoadVector128(&right.M41)));
            return left;
        }

        left.row1 += right.row1;
        left.row2 += right.row2;
        left.row3 += right.row3;
        left.row4 += right.row4;
        return left;
    }

    public static unsafe Matrix4x4Int operator -(Matrix4x4Int left, Matrix4x4Int right)
    {
        if (AdvSimd.IsSupported)
        {
            AdvSimd.Store(&left.M11,
                AdvSimd.Subtract(AdvSimd.LoadVector128(&left.M11), AdvSimd.LoadVector128(&right.M11)));
            AdvSimd.Store(&left.M21,
                AdvSimd.Subtract(AdvSimd.LoadVector128(&left.M21), AdvSimd.LoadVector128(&right.M21)));
            AdvSimd.Store(&left.M31,
                AdvSimd.Subtract(AdvSimd.LoadVector128(&left.M31), AdvSimd.LoadVector128(&right.M31)));
            AdvSimd.Store(&left.M41,
                AdvSimd.Subtract(AdvSimd.LoadVector128(&left.M41), AdvSimd.LoadVector128(&right.M41)));
            return left;
        }

        if (Sse2.IsSupported)
        {
            Sse2.Store(&left.M11,
                Sse2.Subtract(Sse2.LoadVector128(&left.M11), Sse2.LoadVector128(&right.M11)));
            Sse2.Store(&left.M21,
                Sse2.Subtract(Sse2.LoadVector128(&left.M21), Sse2.LoadVector128(&right.M21)));
            Sse2.Store(&left.M31,
                Sse2.Subtract(Sse2.LoadVector128(&left.M31), Sse2.LoadVector128(&right.M31)));
            Sse2.Store(&left.M41,
                Sse2.Subtract(Sse2.LoadVector128(&left.M41), Sse2.LoadVector128(&right.M41)));
            return left;
        }

        left.row1 -= right.row1;
        left.row2 -= right.row2;
        left.row3 -= right.row3;
        left.row4 -= right.row4;
        return left;
    }

    public static unsafe Matrix4x4Int operator -(Matrix4x4Int value)
    {
        if (AdvSimd.IsSupported)
        {
            AdvSimd.Store(&value.M11, AdvSimd.Negate(AdvSimd.LoadVector128(&value.M11)));
            AdvSimd.Store(&value.M21, AdvSimd.Negate(AdvSimd.LoadVector128(&value.M21)));
            AdvSimd.Store(&value.M31, AdvSimd.Negate(AdvSimd.LoadVector128(&value.M31)));
            AdvSimd.Store(&value.M41, AdvSimd.Negate(AdvSimd.LoadVector128(&value.M41)));
            return value;
        }

        if (Sse2.IsSupported)
        {
            var zero = Vector128<int>.Zero;
            Sse2.Store(&value.M11, Sse2.Subtract(zero, Sse2.LoadVector128(&value.M11)));
            Sse2.Store(&value.M21, Sse2.Subtract(zero, Sse2.LoadVector128(&value.M21)));
            Sse2.Store(&value.M31, Sse2.Subtract(zero, Sse2.LoadVector128(&value.M31)));
            Sse2.Store(&value.M41, Sse2.Subtract(zero, Sse2.LoadVector128(&value.M41)));
            return value;
        }

        value.row1 = -value.row1;
        value.row2 = -value.row2;
        value.row3 = -value.row3;
        value.row4 = -value.row4;

        return value;
    }

    public static unsafe Matrix4x4Int operator *(Matrix4x4Int left, Matrix4x4Int right)
    {
        if (AdvSimd.Arm64.IsSupported)
        {
        }

        if (Sse41.IsSupported)
        {
            var row = Sse2.LoadVector128(&left.M11);
            Sse2.Store(&left.M11,
                Sse2.Add(
                    Sse2.Add(Sse41.MultiplyLow(Sse2.Shuffle(row, 0x00), Sse2.LoadVector128(&right.M11)),
                        Sse41.MultiplyLow(Sse2.Shuffle(row, 0x55), Sse2.LoadVector128(&right.M21))),
                    Sse2.Add(Sse41.MultiplyLow(Sse2.Shuffle(row, 0xAA), Sse2.LoadVector128(&right.M31)),
                        Sse41.MultiplyLow(Sse2.Shuffle(row, 0xFF), Sse2.LoadVector128(&right.M41)))));

            row = Sse2.LoadVector128(&left.M21);
            Sse2.Store(&left.M21,
                Sse2.Add(
                    Sse2.Add(Sse41.MultiplyLow(Sse2.Shuffle(row, 0x00), Sse2.LoadVector128(&right.M11)),
                        Sse41.MultiplyLow(Sse2.Shuffle(row, 0x55), Sse2.LoadVector128(&right.M21))),
                    Sse2.Add(Sse41.MultiplyLow(Sse2.Shuffle(row, 0xAA), Sse2.LoadVector128(&right.M31)),
                        Sse41.MultiplyLow(Sse2.Shuffle(row, 0xFF), Sse2.LoadVector128(&right.M41)))));

            row = Sse2.LoadVector128(&left.M31);
            Sse2.Store(&left.M31,
                Sse2.Add(
                    Sse2.Add(Sse41.MultiplyLow(Sse2.Shuffle(row, 0x00), Sse2.LoadVector128(&right.M11)),
                        Sse41.MultiplyLow(Sse2.Shuffle(row, 0x55), Sse2.LoadVector128(&right.M21))),
                    Sse2.Add(Sse41.MultiplyLow(Sse2.Shuffle(row, 0xAA), Sse2.LoadVector128(&right.M31)),
                        Sse41.MultiplyLow(Sse2.Shuffle(row, 0xFF), Sse2.LoadVector128(&right.M41)))));

            row = Sse2.LoadVector128(&left.M41);
            Sse2.Store(&left.M41,
                Sse2.Add(
                    Sse2.Add(Sse41.MultiplyLow(Sse2.Shuffle(row, 0x00), Sse2.LoadVector128(&right.M11)),
                        Sse41.MultiplyLow(Sse2.Shuffle(row, 0x55), Sse2.LoadVector128(&right.M21))),
                    Sse2.Add(Sse41.MultiplyLow(Sse2.Shuffle(row, 0xAA), Sse2.LoadVector128(&right.M31)),
                        Sse41.MultiplyLow(Sse2.Shuffle(row, 0xFF), Sse2.LoadVector128(&right.M41)))));

            return left;
        }

        return new Matrix4x4Int(
            left.M11 * right.M11 + left.M12 * right.M21 + left.M13 * right.M31 + left.M14 * right.M41,
            left.M11 * right.M12 + left.M12 * right.M22 + left.M13 * right.M32 + left.M14 * right.M42,
            left.M11 * right.M13 + left.M12 * right.M23 + left.M13 * right.M33 + left.M14 * right.M43,
            left.M11 * right.M14 + left.M12 * right.M24 + left.M13 * right.M34 + left.M14 * right.M44,
            left.M21 * right.M11 + left.M22 * right.M21 + left.M23 * right.M31 + left.M24 * right.M41,
            left.M21 * right.M12 + left.M22 * right.M22 + left.M23 * right.M32 + left.M24 * right.M42,
            left.M21 * right.M13 + left.M22 * right.M23 + left.M23 * right.M33 + left.M24 * right.M43,
            left.M21 * right.M14 + left.M22 * right.M24 + left.M23 * right.M34 + left.M24 * right.M44,
            left.M31 * right.M11 + left.M32 * right.M21 + left.M33 * right.M31 + left.M34 * right.M41,
            left.M31 * right.M12 + left.M32 * right.M22 + left.M33 * right.M32 + left.M34 * right.M42,
            left.M31 * right.M13 + left.M32 * right.M23 + left.M33 * right.M33 + left.M34 * right.M43,
            left.M31 * right.M14 + left.M32 * right.M24 + left.M33 * right.M34 + left.M34 * right.M44,
            left.M41 * right.M11 + left.M42 * right.M21 + left.M43 * right.M31 + left.M44 * right.M41,
            left.M41 * right.M12 + left.M42 * right.M22 + left.M43 * right.M32 + left.M44 * right.M42,
            left.M41 * right.M13 + left.M42 * right.M23 + left.M43 * right.M33 + left.M44 * right.M43,
            left.M41 * right.M14 + left.M42 * right.M24 + left.M43 * right.M34 + left.M44 * right.M44);
    }

    public static unsafe Matrix4x4Int operator *(Matrix4x4Int matrix, int scalar)
    {
        if (AdvSimd.IsSupported)
        {
            var v = Vector128.Create(scalar);
            AdvSimd.Store(&matrix.M11, AdvSimd.Multiply(AdvSimd.LoadVector128(&matrix.M11), v));
            AdvSimd.Store(&matrix.M21, AdvSimd.Multiply(AdvSimd.LoadVector128(&matrix.M21), v));
            AdvSimd.Store(&matrix.M31, AdvSimd.Multiply(AdvSimd.LoadVector128(&matrix.M31), v));
            AdvSimd.Store(&matrix.M41, AdvSimd.Multiply(AdvSimd.LoadVector128(&matrix.M41), v));
            return matrix;
        }

        if (Sse41.IsSupported)
        {
            var v = Vector128.Create(scalar);
            Sse2.Store(&matrix.M11, Sse41.MultiplyLow(Sse2.LoadVector128(&matrix.M11), v));
            Sse2.Store(&matrix.M21, Sse41.MultiplyLow(Sse2.LoadVector128(&matrix.M21), v));
            Sse2.Store(&matrix.M31, Sse41.MultiplyLow(Sse2.LoadVector128(&matrix.M31), v));
            Sse2.Store(&matrix.M41, Sse41.MultiplyLow(Sse2.LoadVector128(&matrix.M41), v));
            return matrix;
        }

        matrix.row1 *= scalar;
        matrix.row2 *= scalar;
        matrix.row3 *= scalar;
        matrix.row4 *= scalar;
        return matrix;
    }

    public static Matrix4x4Int operator *(int scalar, Matrix4x4Int matrix)
    {
        return matrix * scalar;
    }

    public static unsafe bool operator ==(Matrix4x4Int left, Matrix4x4Int right)
    {
        if (AdvSimd.Arm64.IsSupported)
        {
        }

        if (Sse2.IsSupported)
        {
            return
                Sse2.CompareEqual(Sse2.LoadVector128(&left.M11), Sse2.LoadVector128(&right.M11))
                == Vector128.Create(0xFFFFFFFF).As<uint, int>()
                && Sse2.CompareEqual(Sse2.LoadVector128(&left.M21), Sse2.LoadVector128(&right.M21))
                == Vector128.Create(0xFFFFFFFF).As<uint, int>()
                && Sse2.CompareEqual(Sse2.LoadVector128(&left.M31), Sse2.LoadVector128(&right.M31))
                == Vector128.Create(0xFFFFFFFF).As<uint, int>()
                && Sse2.CompareEqual(Sse2.LoadVector128(&left.M41), Sse2.LoadVector128(&right.M41))
                == Vector128.Create(0xFFFFFFFF).As<uint, int>();
        }

        return left.row1 == right.row1 &&
               left.row2 == right.row2 &&
               left.row3 == right.row3 &&
               left.row4 == right.row4;
    }

    public static unsafe bool operator !=(Matrix4x4Int left, Matrix4x4Int right)
    {
        if (AdvSimd.Arm64.IsSupported)
        {
        }
        else if (Sse2.IsSupported)
        {
            return
                Sse2.CompareEqual(Sse2.LoadVector128(&left.M11), Sse2.LoadVector128(&right.M11))
                != Vector128.Create(0xFFFFFFFF).As<uint, int>()
                || Sse2.CompareEqual(Sse2.LoadVector128(&left.M21), Sse2.LoadVector128(&right.M21))
                != Vector128.Create(0xFFFFFFFF).As<uint, int>()
                || Sse2.CompareEqual(Sse2.LoadVector128(&left.M31), Sse2.LoadVector128(&right.M31))
                != Vector128.Create(0xFFFFFFFF).As<uint, int>()
                || Sse2.CompareEqual(Sse2.LoadVector128(&left.M41), Sse2.LoadVector128(&right.M41))
                != Vector128.Create(0xFFFFFFFF).As<uint, int>();
        }

        return left.row1 != right.row1 ||
               left.row2 != right.row2 ||
               left.row3 != right.row3 ||
               left.row4 != right.row4;
    }


    public static unsafe Matrix4x4Int Transpose(Matrix4x4Int matrix)
    {
        if (AdvSimd.Arm64.IsSupported)
        {
            var r1 = AdvSimd.LoadVector128(&matrix.M11);
            var r3 = AdvSimd.LoadVector128(&matrix.M21);
            var r2 = AdvSimd.LoadVector128(&matrix.M31);
            var r4 = AdvSimd.LoadVector128(&matrix.M41);

            var lR1R3 = AdvSimd.Arm64.ZipLow(r1, r3);
            var hR1R3 = AdvSimd.Arm64.ZipHigh(r1, r3);
            var lR2R4 = AdvSimd.Arm64.ZipLow(r2, r4);
            var hR2R4 = AdvSimd.Arm64.ZipHigh(r2, r4);

            AdvSimd.Store(&matrix.M11, AdvSimd.Arm64.ZipLow(lR1R3, lR2R4));
            AdvSimd.Store(&matrix.M21, AdvSimd.Arm64.ZipHigh(lR1R3, lR2R4));
            AdvSimd.Store(&matrix.M31, AdvSimd.Arm64.ZipLow(hR1R3, hR2R4));
            AdvSimd.Store(&matrix.M41, AdvSimd.Arm64.ZipHigh(hR1R3, hR2R4));

            return matrix;
        }

        if (Sse2.IsSupported)
        {
            var row1 = Sse2.LoadVector128(&matrix.M11);
            var row2 = Sse2.LoadVector128(&matrix.M21);
            var row3 = Sse2.LoadVector128(&matrix.M31);
            var row4 = Sse2.LoadVector128(&matrix.M41);

            var l12 = Sse2.UnpackLow(row1, row2);
            var l34 = Sse2.UnpackLow(row3, row4);
            var h12 = Sse2.UnpackHigh(row1, row2);
            var h34 = Sse2.UnpackHigh(row3, row4);

            var r1 = Sse.MoveLowToHigh(
                Unsafe.As<Vector128<int>, Vector128<float>>(ref l12),
                Unsafe.As<Vector128<int>, Vector128<float>>(ref l34));
            Sse2.Store(
                &matrix.M11,
                Unsafe.As<Vector128<float>, Vector128<int>>(ref r1));

            var r2 = Sse.MoveHighToLow(
                Unsafe.As<Vector128<int>, Vector128<float>>(ref l34),
                Unsafe.As<Vector128<int>, Vector128<float>>(ref l12));
            Sse2.Store(
                &matrix.M21,
                Unsafe.As<Vector128<float>, Vector128<int>>(ref r2));

            var r3 = Sse.MoveLowToHigh(
                Unsafe.As<Vector128<int>, Vector128<float>>(ref h12),
                Unsafe.As<Vector128<int>, Vector128<float>>(ref h34));
            Sse2.Store(
                &matrix.M31,
                Unsafe.As<Vector128<float>, Vector128<int>>(ref r3));

            var r4 = Sse.MoveHighToLow(
                Unsafe.As<Vector128<int>, Vector128<float>>(ref h34),
                Unsafe.As<Vector128<int>, Vector128<float>>(ref h12));
            Sse2.Store(
                &matrix.M41,
                Unsafe.As<Vector128<float>, Vector128<int>>(ref r4));

            return matrix;
        }

        return new Matrix4x4Int(
            matrix.M11, matrix.M21, matrix.M31, matrix.M41,
            matrix.M12, matrix.M22, matrix.M32, matrix.M42,
            matrix.M13, matrix.M23, matrix.M33, matrix.M43,
            matrix.M14, matrix.M24, matrix.M34, matrix.M44);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly override bool Equals(object? obj)
    {
        return obj is Matrix4x4Int other && Equals(other);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Equals(Matrix4x4Int other)
    {
        return row1.Equals(other.row1) &&
               row2.Equals(other.row2) &&
               row3.Equals(other.row3) &&
               row4.Equals(other.row4);
    }

    public readonly int GetDeterminant()
    {
        int a = M11, b = M12, c = M13, d = M14;
        int e = M21, f = M22, g = M23, h = M24;
        int i = M31, j = M32, k = M33, l = M34;
        int m = M41, n = M42, o = M43, p = M44;

        var kp_lo = k * p - l * o;
        var jp_ln = j * p - l * n;
        var jo_kn = j * o - k * n;
        var ip_lm = i * p - l * m;
        var io_km = i * o - k * m;
        var in_jm = i * n - j * m;

        return a * (f * kp_lo - g * jp_ln + h * jo_kn) -
               b * (e * kp_lo - g * ip_lm + h * io_km) +
               c * (e * jp_ln - f * ip_lm + h * in_jm) -
               d * (e * jo_kn - f * io_km + g * in_jm);
    }

    public readonly override int GetHashCode()
    {
        HashCode hash = default;

        hash.Add(row1);
        hash.Add(row2);
        hash.Add(row3);
        hash.Add(row4);

        return hash.ToHashCode();
    }

    public readonly override string ToString() =>
        $"{{ {{M11:{M11} M12:{M12} M13:{M13} M14:{M14}}} {{M21:{M21} M22:{M22} M23:{M23} M24:{M24}}} {{M31:{M31} M32:{M32} M33:{M33} M34:{M34}}} {{M41:{M41} M42:{M42} M43:{M43} M44:{M44}}} }}";
}