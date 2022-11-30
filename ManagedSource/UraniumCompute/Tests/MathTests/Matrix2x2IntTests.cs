using UraniumCompute.Common.Math;

namespace MathTests;

[TestFixture]
public class Matrix2x2IntTests
{
    [TestCase(new[] { 1, 2, 3, 4 }, new[] { 1, 2, 3, 4 }, new[] { 2, 4, 6, 8 })]
    public void Addition(int[] matrix1, int[] matrix2, int[] result)
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Matrix2x2Int(matrix1) + new Matrix2x2Int(matrix2), Is.EqualTo(new Matrix2x2Int(result)));
            Assert.That(new Matrix2x2Int(matrix2) + new Matrix2x2Int(matrix1), Is.EqualTo(new Matrix2x2Int(result)));
        });
    }

    [TestCase(new[] { 1, 2, 3, 4 }, new[] { 1, 2, 3, 4 }, new[] { 0, 0, 0, 0 })]
    [TestCase(new[] { 1, 2, 3, 4 }, new[] { 4, 4, 4, 4 }, new[] { -3, -2, -1, 0 })]
    public void Subtraction(int[] matrix1, int[] matrix2, int[] result)
    {
        var matrix = new Matrix2x2Int(matrix1) - new Matrix2x2Int(matrix2);
        Assert.That(matrix, Is.EqualTo(new Matrix2x2Int(result)));
    }

    [TestCase(new[] { 1, 2, 3, 4 }, new[] { -1, -2, -3, -4 })]
    public void Negation(int[] matrix1, int[] result)
    {
        Assert.That(-new Matrix2x2Int(matrix1), Is.EqualTo(new Matrix2x2Int(result)));
    }

    [TestCase(new[] { 1, 2, 3, 4 }, new[] { 1, 2, 3, 4 }, new[] { 7, 10, 15, 22 })]
    public void Multiplication(int[] matrix1, int[] matrix2, int[] result)
    {
        Assert.That(new Matrix2x2Int(matrix1) * new Matrix2x2Int(matrix2), Is.EqualTo(new Matrix2x2Int(result)));
    }

    [TestCase(new[] { 1, 2, 3, 4 }, 2, new[] { 2, 4, 6, 8 })]
    public void ScalarMultiplication(int[] matrix1, int scalar, int[] result)
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Matrix2x2Int(matrix1) * scalar, Is.EqualTo(new Matrix2x2Int(result)));
            Assert.That(scalar * new Matrix2x2Int(matrix1), Is.EqualTo(new Matrix2x2Int(result)));
        });
    }

    [TestCase(new[] { 1, 2, 3, 4 }, new[] { 1, 3, 2, 4 })]
    public void Transposition(int[] matrix, int[] result)
    {
        Assert.Multiple(() =>
        {
            Assert.That(Matrix2x2Int.Transpose(new Matrix2x2Int(matrix)), Is.EqualTo(new Matrix2x2Int(result)));
            Assert.That(Matrix2x2Int.Transpose(Matrix2x2Int.Transpose(new Matrix2x2Int(matrix))),
                Is.EqualTo(new Matrix2x2Int(matrix)));
        });
    }

    [TestCase(new[] { 0, 0, 0, 0 }, new[] { 0, 0, 0, 0 }, true)]
    [TestCase(new[] { 1, 1, 1, 1 }, new[] { 1, 1, 1, 1 }, true)]
    [TestCase(new[] { 1, 2, 3, 4 }, new[] { 1, 2, 3, 4 }, true)]
    [TestCase(new[] { 1, 2, 3, 4 }, new[] { 0, 2, 3, 4 }, false)]
    [TestCase(new[] { 1, 2, 3, 4 }, new[] { 1, 0, 3, 4 }, false)]
    [TestCase(new[] { 1, 2, 3, 4 }, new[] { 1, 2, 0, 4 }, false)]
    [TestCase(new[] { 1, 2, 3, 4 }, new[] { 1, 2, 3, 0 }, false)]
    public void EqualityCheck(int[] matrix1, int[] matrix2, bool result)
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Matrix2x2Int(matrix1).Equals(new Matrix2x2Int(matrix2)), Is.EqualTo(result));
            Assert.That(new Matrix2x2Int(matrix2).Equals(new Matrix2x2Int(matrix1)), Is.EqualTo(result));
            Assert.That(new Matrix2x2Int(matrix1) == new Matrix2x2Int(matrix2), Is.EqualTo(result));
            Assert.That(new Matrix2x2Int(matrix2) == new Matrix2x2Int(matrix1), Is.EqualTo(result));
            Assert.That(new Matrix2x2Int(matrix1) != new Matrix2x2Int(matrix2), Is.EqualTo(!result));
            Assert.That(new Matrix2x2Int(matrix2) != new Matrix2x2Int(matrix1), Is.EqualTo(!result));
        });
    }
}
