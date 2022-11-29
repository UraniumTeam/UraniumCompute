using UraniumCompute.Common.Math;

namespace MathTests;

[TestFixture]
public class Matrix2x2Tests
{
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
    public void Subtraction(float[] matrix1, float[] matrix2, float[] result)
    {
        Assert.That(new Matrix2x2(matrix1) - new Matrix2x2(matrix2), Is.EqualTo(new Matrix2x2(result)));
    }

    [TestCase(new float[] { 1, 2, 3, 4 }, new float[] { -1, -2, -3, -4 })]
    public void Negative(float[] matrix1, float[] result)
    {
        Assert.That(-new Matrix2x2(matrix1), Is.EqualTo(new Matrix2x2(result)));
    }
    
    [TestCase(new float[] { 1, 2, 3, 4 }, new float[] { 1, 2, 3, 4 }, new float[] { 7, 10, 15, 22 })]
    public void Multiplication(float[] matrix1, float[] matrix2, float[] result)
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Matrix2x2(matrix1) * new Matrix2x2(matrix2), Is.EqualTo(new Matrix2x2(result)));
            Assert.That(new Matrix2x2(matrix2) * new Matrix2x2(matrix1), Is.EqualTo(new Matrix2x2(result)));
        });
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
            Assert.That(Matrix2x2.Transpose(Matrix2x2.Transpose(new Matrix2x2(matrix))), Is.EqualTo(new Matrix2x2(matrix)));
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
