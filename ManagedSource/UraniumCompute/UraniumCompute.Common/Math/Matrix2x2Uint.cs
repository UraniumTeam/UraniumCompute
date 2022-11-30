using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;

namespace UraniumCompute.Common.Math;

[StructLayout(LayoutKind.Explicit)]
public struct Matrix2x2Uint : IEquatable<Matrix2x2Uint>
{
    [FieldOffset(0)] private Vector64<uint> row1;
    [FieldOffset(0)] public uint M11;
    [FieldOffset(sizeof(uint))] public uint M12;

    [FieldOffset(sizeof(uint) * 2)] private Vector64<uint> row2;
    [FieldOffset(sizeof(uint) * 2)] public uint M21;
    [FieldOffset(sizeof(uint) * 3)] public uint M22;

    public Matrix2x2Uint(uint m11, uint m12, uint m21, uint m22)
    {
        M11 = m11;
        M12 = m12;
        M21 = m21;
        M22 = m22;
    }

    public Matrix2x2Uint(ReadOnlySpan<uint> values)
    {
        if (values.Length < 4)
        {
            throw new ArgumentException($"{nameof(values)} must be at least 4 elements in length");
        }

        this = Unsafe.ReadUnaligned<Matrix2x2Uint>(ref Unsafe.As<uint, byte>(ref MemoryMarshal.GetReference(values)));
    }

    private Matrix2x2Uint(Vector64<uint> firstRow, Vector64<uint> secondRow)
    {
        row1 = firstRow;
        row2 = secondRow;
    }

    public static Matrix2x2Uint Identity { get; } = new
    (
        1, 0,
        0, 1
    );

    public uint this[int row, int column]
    {
        get
        {
            if ((uint)row >= 2)
                throw new ArgumentOutOfRangeException();

            var vRow = Unsafe.Add(ref Unsafe.As<uint, Vector2Uint>(ref M11), row);
            return vRow[column];
        }
        set
        {
            if ((uint)row >= 2)
                throw new ArgumentOutOfRangeException();

            ref var vRow = ref Unsafe.Add(ref Unsafe.As<uint, Vector2Uint>(ref M11), row);
            vRow[column] = value;
        }
    }

    public readonly bool IsIdentity => M11 == 1f && M22 == 1f && M12 == 0f && M21 == 0f;

    public static unsafe Matrix2x2Uint operator +(Matrix2x2Uint left, Matrix2x2Uint right)
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
        return Unsafe.ReadUnaligned<Matrix2x2Uint>(ref Unsafe.As<Vector128<uint>, byte>(ref vec));
    }

    public static unsafe Matrix2x2Uint operator -(Matrix2x2Uint left, Matrix2x2Uint right)
    {
        if (AdvSimd.IsSupported)
        {
            AdvSimd.Store(&left.M11,
                AdvSimd.Subtract(AdvSimd.LoadVector64(&left.M11), AdvSimd.LoadVector64(&right.M11)));
            AdvSimd.Store(&left.M21,
                AdvSimd.Subtract(AdvSimd.LoadVector64(&left.M21), AdvSimd.LoadVector64(&right.M21)));
            return left;
        }

        var vec = Vector128.Create(left.row1 - right.row1, left.row2 - right.row2);
        return Unsafe.ReadUnaligned<Matrix2x2Uint>(ref Unsafe.As<Vector128<uint>, byte>(ref vec));
    }

    public static unsafe Matrix2x2Uint operator *(Matrix2x2Uint left, Matrix2x2Uint right)
    {
        if (AdvSimd.Arm64.IsSupported)
        {
            Unsafe.SkipInit(out Matrix2x2Uint result);

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

        var vec = Vector128.Create(
            right.row1 * left.M11 + right.row2 * left.M12,
            right.row1 * left.M21 + right.row2 * left.M22);
        return Unsafe.ReadUnaligned<Matrix2x2Uint>(ref Unsafe.As<Vector128<uint>, byte>(ref vec));
    }

    public static unsafe Matrix2x2Uint operator *(Matrix2x2Uint matrix, uint scalar)
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
        return Unsafe.ReadUnaligned<Matrix2x2Uint>(ref Unsafe.As<Vector128<uint>, byte>(ref vec));
    }


    public static unsafe Matrix2x2Uint operator *(uint scalar, Matrix2x2Uint matrix)
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
        return Unsafe.ReadUnaligned<Matrix2x2Uint>(ref Unsafe.As<Vector128<uint>, byte>(ref vec));
    }

    public static bool operator ==(Matrix2x2Uint left, Matrix2x2Uint right)
    {
        return left.row1 == right.row1 &&
               left.row2 == right.row2;
    }

    public static bool operator !=(Matrix2x2Uint left, Matrix2x2Uint right)
    {
        return left.row1 != right.row1 ||
               left.row2 != right.row2;
    }

    public static unsafe Matrix2x2Uint Transpose(Matrix2x2Uint matrix)
    {
        // if (AdvSimd.Arm64.IsSupported)
        // {
        //     
        // }
        // else if (Sse.IsSupported)
        // {
        //     
        // }

        return new Matrix2x2Uint(matrix.M11, matrix.M21, matrix.M12, matrix.M22);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly override bool Equals(object? obj)
    {
        return obj is Matrix2x2Uint other && Equals(other);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Equals(Matrix2x2Uint other)
    {
        return row1.Equals(other.row1) &&
               row2.Equals(other.row2);
    }

    public readonly uint GetDeterminant() => M11 * M22 - M12 * M21;

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
