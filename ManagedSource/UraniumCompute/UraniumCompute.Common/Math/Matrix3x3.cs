using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace UraniumCompute.Common.Math
{
    [StructLayout(LayoutKind.Explicit)]
    public struct Matrix3x3 : IEquatable<Matrix3x3>
    {
        [FieldOffset(0)] private Vector3 row1;
        [FieldOffset(0)] public float M11;
        [FieldOffset(sizeof(float))] public float M12;
        [FieldOffset(sizeof(float) * 2)] public float M13;

        [FieldOffset(sizeof(float) * 4)] private Vector3 row2;
        [FieldOffset(sizeof(float) * 4)] public float M21;
        [FieldOffset(sizeof(float) * 5)] public float M22;
        [FieldOffset(sizeof(float) * 6)] public float M23;

        [FieldOffset(sizeof(float) * 8)] private Vector3 row3;
        [FieldOffset(sizeof(float) * 8)] public float M31;
        [FieldOffset(sizeof(float) * 9)] public float M32;
        [FieldOffset(sizeof(float) * 10)] public float M33;

        public Matrix3x3(
            float m11, float m12, float m13,
            float m21, float m22, float m23,
            float m31, float m32, float m33)
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

        public Matrix3x3(ReadOnlySpan<float> values)
        {
            if (values.Length < 9)
            {
                throw new ArgumentException($"{nameof(values)} must be at least 9 elements in length");
            }

            this = Unsafe.ReadUnaligned<Matrix3x3>(ref Unsafe.As<float, byte>(ref MemoryMarshal.GetReference(values)));
        }

        private Matrix3x3(Vector3 row1, Vector3 row2, Vector3 row3)
        {
            this.row1 = row1;
            this.row2 = row2;
            this.row3 = row3;
        }

        public static Matrix3x3 Identity { get; } = new
        (
            1, 0, 0,
            0, 1, 0,
            0, 0, 1
        );

        public float this[int row, int column]
        {
            get
            {
                if ((uint)row >= 3)
                    throw new ArgumentOutOfRangeException();

                var vRow = Unsafe.Add(ref Unsafe.As<float, Vector3>(ref M11), row);
                return vRow[column];
            }
            set
            {
                if ((uint)row >= 3)
                    throw new ArgumentOutOfRangeException();

                ref var vRow = ref Unsafe.Add(ref Unsafe.As<float, Vector3>(ref M11), row);
                vRow[column] = value;
            }
        }

        public readonly bool IsIdentity =>
            M11 == 1 && M22 == 1 && M33 == 1 &&
            M12 == 0 && M13 == 0 &&
            M21 == 0 && M23 == 0 &&
            M31 == 0 && M32 == 0;

        public static Matrix3x3 operator +(Matrix3x3 left, Matrix3x3 right)
        {
            left.row1 += right.row1;
            left.row2 += right.row2;
            left.row3 += right.row3;
            return left;
        }

        public static Matrix3x3 operator -(Matrix3x3 left, Matrix3x3 right)
        {
            left.row1 -= right.row1;
            left.row2 -= right.row2;
            left.row3 -= right.row3;
            return left;
        }

        public static Matrix3x3 operator -(Matrix3x3 value)
        {
            value.row1 = -value.row1;
            value.row2 = -value.row2;
            value.row3 = -value.row3;

            return value;
        }

        public static Matrix3x3 operator *(Matrix3x3 left, Matrix3x3 right)
        {
            return new Matrix3x3(
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

        public static Matrix3x3 operator *(Matrix3x3 matrix, float scalar)
        {
            matrix.row1 *= scalar;
            matrix.row2 *= scalar;
            matrix.row3 *= scalar;
            return matrix;
        }

        public static Matrix3x3 operator *(float scalar, Matrix3x3 matrix)
        {
            return matrix * scalar;
        }

        public static bool operator ==(Matrix3x3 left, Matrix3x3 right)
        {
            return left.row1 == right.row1 &&
                   left.row2 == right.row2 &&
                   left.row3 == right.row3;
        }

        public static bool operator !=(Matrix3x3 left, Matrix3x3 right)
        {
            return left.row1 != right.row1 ||
                   left.row2 != right.row2 ||
                   left.row3 != right.row3;
        }


        public static Matrix3x3 Transpose(Matrix3x3 matrix)
        {
            return new Matrix3x3(
                matrix.M11, matrix.M21, matrix.M31,
                matrix.M12, matrix.M22, matrix.M32,
                matrix.M13, matrix.M23, matrix.M33);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override bool Equals(object? obj)
        {
            return obj is Matrix3x3 other && Equals(other);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(Matrix3x3 other)
        {
            return row1.Equals(other.row1) &&
                   row2.Equals(other.row2) &&
                   row3.Equals(other.row3);
        }

        public readonly float GetDeterminant()
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
}
