using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;

namespace UraniumCompute.Common.Math;

[StructLayout(LayoutKind.Explicit)]
public struct Matrix2x2Int : IEquatable<Matrix2x2Int>
{
    [FieldOffset(0)] private Vector64<int> row1;
    [FieldOffset(0)] public int M11;
    [FieldOffset(sizeof(int))] public int M12;

    [FieldOffset(sizeof(int) * 2)] private Vector64<int> row2;
    [FieldOffset(sizeof(int) * 2)] public int M21;
    [FieldOffset(sizeof(int) * 3)] public int M22;

    public Matrix2x2Int(int m11, int m12, int m21, int m22)
    {
        M11 = m11;
        M12 = m12;
        M21 = m21;
        M22 = m22;
    }

    public Matrix2x2Int(ReadOnlySpan<int> values)
    {
        if (values.Length < 4)
        {
            throw new ArgumentException($"{nameof(values)} must be at least 4 elements in length");
        }

        this = Unsafe.ReadUnaligned<Matrix2x2Int>(ref Unsafe.As<int, byte>(ref MemoryMarshal.GetReference(values)));
    }

    private Matrix2x2Int(Vector64<int> firstRow, Vector64<int> secondRow)
    {
        row1 = firstRow;
        row2 = secondRow;
    }

    public static Matrix2x2Int Identity { get; } = new
    (
        1, 0,
        0, 1
    );

    public int this[int row, int column]
    {
        get
        {
            if ((uint)row >= 2)
                throw new ArgumentOutOfRangeException();

            var vRow = Unsafe.Add(ref Unsafe.As<int, Vector2Int>(ref M11), row);
            return vRow[column];
        }
        set
        {
            if ((uint)row >= 2)
                throw new ArgumentOutOfRangeException();

            ref var vRow = ref Unsafe.Add(ref Unsafe.As<int, Vector2Int>(ref M11), row);
            vRow[column] = value;
        }
    }

    public readonly bool IsIdentity => M11 == 1f && M22 == 1f && M12 == 0f && M21 == 0f;

    public static unsafe Matrix2x2Int operator +(Matrix2x2Int left, Matrix2x2Int right)
    {
        if (AdvSimd.IsSupported)
        {
            AdvSimd.Store(&left.M11,
                AdvSimd.Add(AdvSimd.LoadVector64(&left.M11), AdvSimd.LoadVector64(&right.M11)));
            AdvSimd.Store(&left.M21,
                AdvSimd.Add(AdvSimd.LoadVector64(&left.M21), AdvSimd.LoadVector64(&right.M21)));
            return left;
        }

        if (Sse2.IsSupported)
        {
            Sse2.Store(&left.M11, Sse2.Add(Sse2.LoadVector128(&left.M11), Sse2.LoadVector128(&right.M11)));
            return left;
        }

        var vec = Vector128.Create(left.row1 + right.row1, left.row2 + right.row2);
        return Unsafe.ReadUnaligned<Matrix2x2Int>(ref Unsafe.As<Vector128<int>, byte>(ref vec));
    }

    public static unsafe Matrix2x2Int operator -(Matrix2x2Int left, Matrix2x2Int right)
    {
        if (AdvSimd.IsSupported)
        {
            AdvSimd.Store(&left.M11,
                AdvSimd.Subtract(AdvSimd.LoadVector64(&left.M11), AdvSimd.LoadVector64(&right.M11)));
            AdvSimd.Store(&left.M21,
                AdvSimd.Subtract(AdvSimd.LoadVector64(&left.M21), AdvSimd.LoadVector64(&right.M21)));
            return left;
        }

        if (Sse2.IsSupported)
        {
            Sse2.Store(&left.M11, Sse2.Subtract(Sse2.LoadVector128(&left.M11), Sse2.LoadVector128(&right.M11)));
            return left;
        }

        var vec = Vector128.Create(left.row1 - right.row1, left.row2 - right.row2);
        return Unsafe.ReadUnaligned<Matrix2x2Int>(ref Unsafe.As<Vector128<int>, byte>(ref vec));
    }

    public static unsafe Matrix2x2Int operator -(Matrix2x2Int value)
    {
        if (AdvSimd.IsSupported)
        {
            AdvSimd.Store(&value.M11, AdvSimd.Negate(AdvSimd.LoadVector64(&value.M11)));
            AdvSimd.Store(&value.M21, AdvSimd.Negate(AdvSimd.LoadVector64(&value.M21)));
            return value;
        }

        if (Sse2.IsSupported)
        {
            var zero = Vector128<int>.Zero;
            Sse2.Store(&value.M11, Sse2.Subtract(zero, Sse2.LoadVector128(&value.M11)));
            return value;
        }

        var vec = -Vector128.Create(value.row1, value.row2);
        return Unsafe.ReadUnaligned<Matrix2x2Int>(ref Unsafe.As<Vector128<int>, byte>(ref vec));
    }

    public static unsafe Matrix2x2Int operator *(Matrix2x2Int left, Matrix2x2Int right)
    {
        if (AdvSimd.Arm64.IsSupported)
        {
            Unsafe.SkipInit(out Matrix2x2Int result);

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

        if (Sse2.IsSupported)
        {
            // var zero = Vector128<int>.Zero;
            // var l = Sse2.LoadVector128(&left.M11);
            // var r = Sse2.LoadVector128(&right.M11);
            // var m2ShiftLeft = Sse2.Shuffle(r, 0x4E);
            //var m2ShiftLet = Sse2.UnpackHigh()Shuffle(r, 0x4E);
            // Sse2.Store(
            //     &left.M11,
            //     Sse2.Add(
            //         Sse2.Add(, r),
            //             Sse2.Multiply(, m2ShiftLeft)),
            //         Sse2.Add(, m2ShiftLeft),
            //             Sse2.Multiply(, r))));
//            return left;
        }

        var vec = Vector128.Create(
            right.row1 * left.M11 + right.row2 * left.M12,
            right.row1 * left.M21 + right.row2 * left.M22);
        return Unsafe.ReadUnaligned<Matrix2x2Int>(ref Unsafe.As<Vector128<int>, byte>(ref vec));
    }

    public static unsafe Matrix2x2Int operator *(Matrix2x2Int matrix, int scalar)
    {
        if (AdvSimd.IsSupported)
        {
            var v = Vector64.Create(scalar);
            AdvSimd.Store(&matrix.M11, AdvSimd.Multiply(AdvSimd.LoadVector64(&matrix.M11), v));
            AdvSimd.Store(&matrix.M21, AdvSimd.Multiply(AdvSimd.LoadVector64(&matrix.M21), v));
            return matrix;
        }

        if (Sse41.IsSupported)
        {
            var v = Vector128.Create(scalar);
            Sse2.Store(&matrix.M11, Sse41.MultiplyLow(Sse2.LoadVector128(&matrix.M11), v));
            return matrix;
        }

        var vec = Vector128.Create(matrix.row1 * scalar, matrix.row2 * scalar);
        return Unsafe.ReadUnaligned<Matrix2x2Int>(ref Unsafe.As<Vector128<int>, byte>(ref vec));
    }


    public static unsafe Matrix2x2Int operator *(int scalar, Matrix2x2Int matrix)
    {
        if (AdvSimd.IsSupported)
        {
            var v = Vector64.Create(scalar);
            AdvSimd.Store(&matrix.M11, AdvSimd.Multiply(AdvSimd.LoadVector64(&matrix.M11), v));
            AdvSimd.Store(&matrix.M21, AdvSimd.Multiply(AdvSimd.LoadVector64(&matrix.M21), v));
            return matrix;
        }

        if (Sse41.IsSupported)
        {
            var v = Vector128.Create(scalar);
            Sse2.Store(&matrix.M11, Sse41.MultiplyLow(Sse2.LoadVector128(&matrix.M11), v));
            return matrix;
        }

        var vec = Vector128.Create(matrix.row1 * scalar, matrix.row2 * scalar);
        return Unsafe.ReadUnaligned<Matrix2x2Int>(ref Unsafe.As<Vector128<int>, byte>(ref vec));
    }

    public static bool operator ==(Matrix2x2Int left, Matrix2x2Int right)
    {
        return left.row1 == right.row1 &&
               left.row2 == right.row2;
    }

    public static bool operator !=(Matrix2x2Int left, Matrix2x2Int right)
    {
        return left.row1 != right.row1 ||
               left.row2 != right.row2;
    }

    public static unsafe Matrix2x2Int Transpose(Matrix2x2Int matrix)
    {
        // if (AdvSimd.Arm64.IsSupported)
        // {
        //     
        // }
        // else if (Sse.IsSupported)
        // {
        //     
        // }

        return new Matrix2x2Int(matrix.M11, matrix.M21, matrix.M12, matrix.M22);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly override bool Equals(object? obj)
    {
        return obj is Matrix2x2Int other && Equals(other);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Equals(Matrix2x2Int other)
    {
        if (Vector64.IsHardwareAccelerated)
        {
            return Vector64.LoadUnsafe(ref Unsafe.AsRef(in M11)).Equals(Vector64.LoadUnsafe(ref other.M11))
                   && Vector64.LoadUnsafe(ref Unsafe.AsRef(in M21)).Equals(Vector64.LoadUnsafe(ref other.M21));
        }

        return row1.Equals(other.row1) &&
               row2.Equals(other.row2);
    }

    public readonly int GetDeterminant() => M11 * M22 - M12 * M21;

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
