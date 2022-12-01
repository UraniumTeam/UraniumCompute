using UraniumCompute.Common.Math;

namespace MathTests;

[TestFixture]
public class Matrix3x3Tests
{
    // temp comparer
    private class Matrix3x3Comparer : IComparer<Matrix3x3>
    {
        public int Compare(Matrix3x3 x, Matrix3x3 y)
        {
            const float epsilon = 0.001f;
            var m11Comparison = x.M11 - y.M11 > epsilon;
            var m12Comparison = x.M12 - y.M12 > epsilon;
            var m13Comparison = x.M13 - y.M13 > epsilon;
            var m21Comparison = x.M21 - y.M21 > epsilon;
            var m22Comparison = x.M22 - y.M22 > epsilon;
            var m23Comparison = x.M23 - y.M23 > epsilon;
            var m31Comparison = x.M31 - y.M31 > epsilon;
            var m32Comparison = x.M32 - y.M32 > epsilon;
            var m33Comparison = x.M33 - y.M33 > epsilon;
            if (m11Comparison || m12Comparison || m13Comparison ||
                m21Comparison || m22Comparison || m23Comparison ||
                m31Comparison || m32Comparison || m33Comparison)
                return -1;
            m11Comparison = x.M11 - y.M11 < epsilon;
            m12Comparison = x.M12 - y.M12 < epsilon;
            m13Comparison = x.M13 - y.M13 < epsilon;
            m21Comparison = x.M21 - y.M21 < epsilon;
            m22Comparison = x.M22 - y.M22 < epsilon;
            m23Comparison = x.M23 - y.M23 < epsilon;
            m31Comparison = x.M31 - y.M31 < epsilon;
            m32Comparison = x.M32 - y.M32 < epsilon;
            m33Comparison = x.M33 - y.M33 < epsilon;
            if (m11Comparison || m12Comparison || m13Comparison ||
                m21Comparison || m22Comparison || m23Comparison ||
                m31Comparison || m32Comparison || m33Comparison)
                return 1;
            return 0;
        }
    }


    private static readonly Matrix3x3Comparer Comparer = new();

    [TestCase(
        new float[]
        {
            1, 2, 3,
            1, 2, 3,
            1, 2, 3
        },
        new float[]
        {
            1, 2, 3,
            1, 2, 3,
            1, 2, 3
        },
        new float[]
        {
            2, 4, 6,
            2, 4, 6,
            2, 4, 6
        })]
    public void Addition(float[] matrix1, float[] matrix2, float[] result)
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Matrix3x3(matrix1) + new Matrix3x3(matrix2), Is.EqualTo(new Matrix3x3(result)));
            Assert.That(new Matrix3x3(matrix2) + new Matrix3x3(matrix1), Is.EqualTo(new Matrix3x3(result)));
        });
    }

    [TestCase(
        new float[]
        {
            1, 2, 3,
            1, 2, 3,
            1, 2, 3
        },
        new float[]
        {
            1, 2, 3,
            1, 2, 3,
            1, 2, 3
        },
        new float[]
        {
            0, 0, 0,
            0, 0, 0,
            0, 0, 0
        })]
    [TestCase(
        new[]
        {
            1.2f, 8.7f, 3.3f,
            4.4f, 2, 3,
            1, 2, 3
        },
        new[]
        {
            2.5f, 5.1f, 3.9f,
            4.5f, 2, 3,
            1, 2, 3
        },
        new[]
        {
            -1.3f, 3.6f, -0.6f,
            -0.1f, 0, 0,
            0, 0, 0
        })]
    public void Subtraction(float[] matrix1, float[] matrix2, float[] result)
    {
        var matrix = new Matrix3x3(matrix1) - new Matrix3x3(matrix2);
        Assert.That(matrix, Is.EqualTo(new Matrix3x3(result)).Using(Comparer));
    }

    [TestCase(
        new float[]
        {
            1, 2, 3,
            1, 2, 3,
            1, 2, 3
        },
        new float[]
        {
            -1, -2, -3,
            -1, -2, -3,
            -1, -2, -3
        })]
    public void Negation(float[] matrix1, float[] result)
    {
        Assert.That(-new Matrix3x3(matrix1), Is.EqualTo(new Matrix3x3(result)));
    }

    [TestCase(
        new float[]
        {
            1, 2, 3,
            1, 2, 3,
            1, 2, 3
        },
        new float[]
        {
            1, 2, 3,
            1, 2, 3,
            1, 2, 3
        },
        new float[]
        {
            6, 12, 18,
            6, 12, 18,
            6, 12, 18
        })]
    [TestCase(
        new[]
        {
            1.2f, 2.7f, 3.3f,
            4.4f, 2, 3,
            1, 2, 3
        },
        new[]
        {
            2.8f, 5.1f, 3.9f,
            4.5f, 2, 3,
            1, 2, 3
        },
        new[]
        {
            18.81f, 18.12f, 22.68f,
            24.32f, 32.44f, 32.16f,
            14.8f, 15.1f, 18.9f
        })]
    public void Multiplication(float[] matrix1, float[] matrix2, float[] result)
    {
        Assert.That(new Matrix3x3(matrix1) * new Matrix3x3(matrix2), Is.EqualTo(new Matrix3x3(result)).Using(Comparer));
    }

    [TestCase(
        new float[]
        {
            1, 2, 3,
            1, 2, 3,
            1, 2, 3
        },
        2,
        new float[]
        {
            2, 4, 6,
            2, 4, 6,
            2, 4, 6
        })]
    public void ScalarMultiplication(float[] matrix1, float scalar, float[] result)
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Matrix3x3(matrix1) * scalar, Is.EqualTo(new Matrix3x3(result)));
            Assert.That(scalar * new Matrix3x3(matrix1), Is.EqualTo(new Matrix3x3(result)));
        });
    }

    [TestCase(
        new float[]
        {
            1, 2, 3,
            1, 2, 3,
            1, 2, 3
        },
        new float[]
        {
            1, 1, 1,
            2, 2, 2,
            3, 3, 3
        })]
    public void Transposition(float[] matrix, float[] result)
    {
        Assert.Multiple(() =>
        {
            Assert.That(Matrix3x3.Transpose(new Matrix3x3(matrix)), Is.EqualTo(new Matrix3x3(result)));
            Assert.That(Matrix3x3.Transpose(Matrix3x3.Transpose(new Matrix3x3(matrix))),
                Is.EqualTo(new Matrix3x3(matrix)));
        });
    }

    [TestCase(
        new float[]
        {
            0, 0, 0,
            0, 0, 0,
            0, 0, 0
        },
        new float[]
        {
            0, 0, 0,
            0, 0, 0,
            0, 0, 0
        },
        true)]
    [TestCase(
        new float[]
        {
            1, 2, 3,
            1, 2, 3,
            1, 2, 3
        },
        new float[]
        {
            1, 2, 3,
            1, 2, 3,
            1, 2, 3
        },
        true)]
    [TestCase(
        new float[]
        {
            0, 2, 3,
            1, 2, 3,
            1, 2, 3
        },
        new float[]
        {
            1, 2, 3,
            1, 2, 3,
            1, 2, 3
        },
        false)]
    [TestCase(
        new float[]
        {
            1, 2, 3,
            1, 0, 3,
            1, 2, 3
        },
        new float[]
        {
            1, 2, 3,
            1, 2, 3,
            1, 2, 3
        },
        false)]
    [TestCase(
        new float[]
        {
            1, 2, 3,
            1, 2, 3,
            1, 2, 0
        },
        new float[]
        {
            1, 2, 3,
            1, 2, 3,
            1, 2, 3
        },
        false)]
    public void EqualityCheck(float[] matrix1, float[] matrix2, bool result)
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Matrix3x3(matrix1).Equals(new Matrix3x3(matrix2)), Is.EqualTo(result));
            Assert.That(new Matrix3x3(matrix2).Equals(new Matrix3x3(matrix1)), Is.EqualTo(result));
            Assert.That(new Matrix3x3(matrix1) == new Matrix3x3(matrix2), Is.EqualTo(result));
            Assert.That(new Matrix3x3(matrix2) == new Matrix3x3(matrix1), Is.EqualTo(result));
            Assert.That(new Matrix3x3(matrix1) != new Matrix3x3(matrix2), Is.EqualTo(!result));
            Assert.That(new Matrix3x3(matrix2) != new Matrix3x3(matrix1), Is.EqualTo(!result));
        });
    }
}
