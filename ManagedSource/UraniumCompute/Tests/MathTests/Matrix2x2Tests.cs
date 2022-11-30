using UraniumCompute.Common.Math;

namespace MathTests;

[TestFixture]
public class Matrix2x2Tests
{
    // temp comparer
    private class Matrix2x2Comparer : IComparer<Matrix2x2>
    {
        public int Compare(Matrix2x2 x, Matrix2x2 y)
        {
            const float epsilon = 0.0001f;
            var m11Comparison = x.M11 - y.M11 > epsilon;
            var m12Comparison = x.M12 - y.M12 > epsilon;
            var m21Comparison = x.M21 - y.M21 > epsilon;
            var m22Comparison = x.M22 - y.M22 > epsilon;
            if (m11Comparison || m12Comparison || m21Comparison || m22Comparison)
                return -1;
            m11Comparison = y.M11 - x.M11 > epsilon;
            m12Comparison = y.M12 - x.M12 > epsilon;
            m21Comparison = y.M21 - x.M21 > epsilon;
            m22Comparison = y.M22 - x.M22 > epsilon;
            if (m11Comparison || m12Comparison || m21Comparison || m22Comparison)
                return 1;
            return 0;
        }
    }


    private static readonly Matrix2x2Comparer Comparer = new();

    [TestCase(new float[] { 1, 2, 3, 4 }, new float[] { 1, 2, 3, 4 }, new float[] { 2, 4, 6, 8 })]
    public void Addition(float[] matrix1, float[] matrix2, float[] result)
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Matrix2x2(matrix1) + new Matrix2x2(matrix2), Is.EqualTo(new Matrix2x2(result)));
            Assert.That(new Matrix2x2(matrix2) + new Matrix2x2(matrix1), Is.EqualTo(new Matrix2x2(result)));
        });
    }

    [TestCase(new float[] { 1, 2, 3, 4 }, new float[] { 1, 2, 3, 4 }, new float[] { 0, 0, 0, 0 })]
    [TestCase(new[] { 1.2f, 8.7f, 3.3f, 4.4f }, new[] { 2.5f, 5.1f, 3.9f, 4.5f }, new[] { -1.3f, 3.6f, -0.6f, -0.1f })]
    public void Subtraction(float[] matrix1, float[] matrix2, float[] result)
    {
        var matrix = new Matrix2x2(matrix1) - new Matrix2x2(matrix2);
        Assert.That(matrix, Is.EqualTo(new Matrix2x2(result)).Using(Comparer));
    }

    [TestCase(new float[] { 1, 2, 3, 4 }, new float[] { -1, -2, -3, -4 })]
    public void Negative(float[] matrix1, float[] result)
    {
        Assert.That(-new Matrix2x2(matrix1), Is.EqualTo(new Matrix2x2(result)));
    }

    [TestCase(new float[] { 1, 2, 3, 4 }, new float[] { 1, 2, 3, 4 }, new float[] { 7, 10, 15, 22 })]
    [TestCase(new[] { 1.2f, 2.7f, 3.3f, 4.4f }, new[] { 2.8f, 5.1f, 3.9f, 4.5f },
        new[] { 13.89f, 18.27f, 26.4f, 36.63f })]
    public void Multiplication(float[] matrix1, float[] matrix2, float[] result)
    {
        Assert.That(new Matrix2x2(matrix1) * new Matrix2x2(matrix2), Is.EqualTo(new Matrix2x2(result)).Using(Comparer));
    }

    [TestCase(new float[] { 1, 2, 3, 4 }, 2, new float[] { 2, 4, 6, 8 })]
    public void ScalarMultiplication(float[] matrix1, float scalar, float[] result)
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Matrix2x2(matrix1) * scalar, Is.EqualTo(new Matrix2x2(result)));
            Assert.That(scalar * new Matrix2x2(matrix1), Is.EqualTo(new Matrix2x2(result)));
        });
    }

    [TestCase(new float[] { 1, 2, 3, 4 }, new float[] { 1, 3, 2, 4 })]
    public void Transposition(float[] matrix, float[] result)
    {
        Assert.Multiple(() =>
        {
            Assert.That(Matrix2x2.Transpose(new Matrix2x2(matrix)), Is.EqualTo(new Matrix2x2(result)));
            Assert.That(Matrix2x2.Transpose(Matrix2x2.Transpose(new Matrix2x2(matrix))),
                Is.EqualTo(new Matrix2x2(matrix)));
        });
    }

    [TestCase(new float[] { 0, 0, 0, 0 }, new float[] { 0, 0, 0, 0 }, true)]
    [TestCase(new float[] { 1, 1, 1, 1 }, new float[] { 1, 1, 1, 1 }, true)]
    [TestCase(new float[] { 1, 2, 3, 4 }, new float[] { 1, 2, 3, 4 }, true)]
    [TestCase(new float[] { 1, 2, 3, 4 }, new float[] { 0, 2, 3, 4 }, false)]
    [TestCase(new float[] { 1, 2, 3, 4 }, new float[] { 1, 0, 3, 4 }, false)]
    [TestCase(new float[] { 1, 2, 3, 4 }, new float[] { 1, 2, 0, 4 }, false)]
    [TestCase(new float[] { 1, 2, 3, 4 }, new float[] { 1, 2, 3, 0 }, false)]
    public void EqualityCheck(float[] matrix1, float[] matrix2, bool result)
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Matrix2x2(matrix1).Equals(new Matrix2x2(matrix2)), Is.EqualTo(result));
            Assert.That(new Matrix2x2(matrix2).Equals(new Matrix2x2(matrix1)), Is.EqualTo(result));
            Assert.That(new Matrix2x2(matrix1) == new Matrix2x2(matrix2), Is.EqualTo(result));
            Assert.That(new Matrix2x2(matrix2) == new Matrix2x2(matrix1), Is.EqualTo(result));
            Assert.That(new Matrix2x2(matrix1) != new Matrix2x2(matrix2), Is.EqualTo(!result));
            Assert.That(new Matrix2x2(matrix2) != new Matrix2x2(matrix1), Is.EqualTo(!result));
        });
    }
}
