using UraniumCompute.Common.Math;

namespace MathTests;

[TestFixture]
public class Matrix2x2UintTests
{
    [TestCase(new uint[] { 1, 2, 3, 4 }, new uint[] { 1, 2, 3, 4 }, new uint[] { 2, 4, 6, 8 })]
    public void Addition(uint[] matrix1, uint[] matrix2, uint[] result)
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Matrix2x2Uint(matrix1) + new Matrix2x2Uint(matrix2), Is.EqualTo(new Matrix2x2Uint(result)));
            Assert.That(new Matrix2x2Uint(matrix2) + new Matrix2x2Uint(matrix1), Is.EqualTo(new Matrix2x2Uint(result)));
        });
    }

    [TestCase(new uint[] { 1, 2, 3, 4 }, new uint[] { 1, 2, 3, 4 }, new uint[] { 0, 0, 0, 0 })]
    public void Subtraction(uint[] matrix1, uint[] matrix2, uint[] result)
    {
        var matrix = new Matrix2x2Uint(matrix1) - new Matrix2x2Uint(matrix2);
        Assert.That(matrix, Is.EqualTo(new Matrix2x2Uint(result)));
    }

    [TestCase(new uint[] { 1, 2, 3, 4 }, new uint[] { 1, 2, 3, 4 }, new uint[] { 7, 10, 15, 22 })]
    public void Multiplication(uint[] matrix1, uint[] matrix2, uint[] result)
    {
        Assert.That(new Matrix2x2Uint(matrix1) * new Matrix2x2Uint(matrix2), Is.EqualTo(new Matrix2x2Uint(result)));
    }

    [TestCase(new uint[] { 1, 2, 3, 4 }, 2u, new uint[] { 2, 4, 6, 8 })]
    [TestCase(new uint[] { 1, 2, 3, 4 }, 200000000u, new uint[] { 200000000, 400000000, 600000000, 800000000 })]
    public void ScalarMultiplication(uint[] matrix1, uint scalar, uint[] result)
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Matrix2x2Uint(matrix1) * scalar, Is.EqualTo(new Matrix2x2Uint(result)));
            Assert.That(scalar * new Matrix2x2Uint(matrix1), Is.EqualTo(new Matrix2x2Uint(result)));
        });
    }

    [TestCase(new uint[] { 1, 2, 3, 4 }, new uint[] { 1, 3, 2, 4 })]
    public void Transposition(uint[] matrix, uint[] result)
    {
        Assert.Multiple(() =>
        {
            Assert.That(Matrix2x2Uint.Transpose(new Matrix2x2Uint(matrix)), Is.EqualTo(new Matrix2x2Uint(result)));
            Assert.That(Matrix2x2Uint.Transpose(Matrix2x2Uint.Transpose(new Matrix2x2Uint(matrix))),
                Is.EqualTo(new Matrix2x2Uint(matrix)));
        });
    }

    [TestCase(new uint[] { 0, 0, 0, 0 }, new uint[] { 0, 0, 0, 0 }, true)]
    [TestCase(new uint[] { 1, 1, 1, 1 }, new uint[] { 1, 1, 1, 1 }, true)]
    [TestCase(new uint[] { 1, 2, 3, 4 }, new uint[] { 1, 2, 3, 4 }, true)]
    [TestCase(new uint[] { 1, 2, 3, 4 }, new uint[] { 0, 2, 3, 4 }, false)]
    [TestCase(new uint[] { 1, 2, 3, 4 }, new uint[] { 1, 0, 3, 4 }, false)]
    [TestCase(new uint[] { 1, 2, 3, 4 }, new uint[] { 1, 2, 0, 4 }, false)]
    [TestCase(new uint[] { 1, 2, 3, 4 }, new uint[] { 1, 2, 3, 0 }, false)]
    public void EqualityCheck(uint[] matrix1, uint[] matrix2, bool result)
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Matrix2x2Uint(matrix1).Equals(new Matrix2x2Uint(matrix2)), Is.EqualTo(result));
            Assert.That(new Matrix2x2Uint(matrix2).Equals(new Matrix2x2Uint(matrix1)), Is.EqualTo(result));
            Assert.That(new Matrix2x2Uint(matrix1) == new Matrix2x2Uint(matrix2), Is.EqualTo(result));
            Assert.That(new Matrix2x2Uint(matrix2) == new Matrix2x2Uint(matrix1), Is.EqualTo(result));
            Assert.That(new Matrix2x2Uint(matrix1) != new Matrix2x2Uint(matrix2), Is.EqualTo(!result));
            Assert.That(new Matrix2x2Uint(matrix2) != new Matrix2x2Uint(matrix1), Is.EqualTo(!result));
        });
    }
}
