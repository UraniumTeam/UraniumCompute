using UraniumCompute.Common.Math;

namespace MathTests;

[TestFixture]
public class Matrix4x4IntTests
{
    [TestCase(
        new[]
        {
            1, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 4
        },
        new[]
        {
            1, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 4
        },
        new[]
        {
            2, 4, 6, 8,
            2, 4, 6, 8,
            2, 4, 6, 8,
            2, 4, 6, 8
        })]
    public void Addition(int[] matrix1, int[] matrix2, int[] result)
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Matrix4x4Int(matrix1) + new Matrix4x4Int(matrix2), Is.EqualTo(new Matrix4x4Int(result)));
            Assert.That(new Matrix4x4Int(matrix2) + new Matrix4x4Int(matrix1), Is.EqualTo(new Matrix4x4Int(result)));
        });
    }

    [TestCase(
        new[]
        {
            1, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 4
        },
        new[]
        {
            1, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 4
        },
        new[]
        {
            0, 0, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0
        })]
    [TestCase(
        new[]
        {
            1, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 4
        },
        new[]
        {
            4, 4, 4, 4,
            4, 4, 4, 4,
            4, 4, 4, 4,
            4, 4, 4, 4
        },
        new[]
        {
            -3, -2, -1, 0,
            -3, -2, -1, 0,
            -3, -2, -1, 0,
            -3, -2, -1, 0
        })]
    public void Subtraction(int[] matrix1, int[] matrix2, int[] result)
    {
        var matrix = new Matrix4x4Int(matrix1) - new Matrix4x4Int(matrix2);
        Assert.That(matrix, Is.EqualTo(new Matrix4x4Int(result)));
    }

    [TestCase(
        new[]
        {
            1, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 4
        },
        new[]
        {
            -1, -2, -3, -4,
            -1, -2, -3, -4,
            -1, -2, -3, -4,
            -1, -2, -3, -4
        })]
    public void Negation(int[] matrix1, int[] result)
    {
        Assert.That(-new Matrix4x4Int(matrix1), Is.EqualTo(new Matrix4x4Int(result)));
    }

    [TestCase(
        new[]
        {
            1, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 4
        },
        new[]
        {
            1, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 4
        },
        new[]
        {
            10, 20, 30, 40,
            10, 20, 30, 40,
            10, 20, 30, 40,
            10, 20, 30, 40
        })]
    public void Multiplication(int[] matrix1, int[] matrix2, int[] result)
    {
        Assert.That(new Matrix4x4Int(matrix1) * new Matrix4x4Int(matrix2), Is.EqualTo(new Matrix4x4Int(result)));
    }

    [TestCase(
        new[]
        {
            1, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 4
        },
        2,
        new[]
        {
            2, 4, 6, 8,
            2, 4, 6, 8,
            2, 4, 6, 8,
            2, 4, 6, 8
        })]
    [TestCase(
        new[]
        {
            1, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 4
        },
        200000000,
        new[]
        {
            200000000, 400000000, 600000000, 800000000,
            200000000, 400000000, 600000000, 800000000,
            200000000, 400000000, 600000000, 800000000,
            200000000, 400000000, 600000000, 800000000
        })]
    public void ScalarMultiplication(int[] matrix1, int scalar, int[] result)
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Matrix4x4Int(matrix1) * scalar, Is.EqualTo(new Matrix4x4Int(result)));
            Assert.That(scalar * new Matrix4x4Int(matrix1), Is.EqualTo(new Matrix4x4Int(result)));
        });
    }

    [TestCase(
        new[]
        {
            1, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 4
        },
        new[]
        {
            1, 1, 1, 1,
            2, 2, 2, 2,
            3, 3, 3, 3,
            4, 4, 4, 4
        })]
    public void Transposition(int[] matrix, int[] result)
    {
        Assert.Multiple(() =>
        {
            Assert.That(Matrix4x4Int.Transpose(new Matrix4x4Int(matrix)), Is.EqualTo(new Matrix4x4Int(result)));
            Assert.That(Matrix4x4Int.Transpose(Matrix4x4Int.Transpose(new Matrix4x4Int(matrix))),
                Is.EqualTo(new Matrix4x4Int(matrix)));
        });
    }

    [TestCase(
        new[]
        {
            0, 0, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0
        },
        new[]
        {
            0, 0, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0
        },
        true)]
    [TestCase(
        new[]
        {
            1, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 4
        },
        new[]
        {
            1, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 4
        },
        true)]
    [TestCase(
        new[]
        {
            0, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 4
        },
        new[]
        {
            1, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 4
        },
        false)]
    [TestCase(
        new[]
        {
            1, 2, 3, 4,
            1, 0, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 4
        },
        new[]
        {
            1, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 4
        },
        false)]
    [TestCase(
        new[]
        {
            1, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 0, 4,
            1, 2, 3, 4
        },
        new[]
        {
            1, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 4
        },
        false)]
    [TestCase(
        new[]
        {
            1, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 0
        },
        new[]
        {
            1, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 4
        },
        false)]
    public void EqualityCheck(int[] matrix1, int[] matrix2, bool result)
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Matrix4x4Int(matrix1).Equals(new Matrix4x4Int(matrix2)), Is.EqualTo(result));
            Assert.That(new Matrix4x4Int(matrix2).Equals(new Matrix4x4Int(matrix1)), Is.EqualTo(result));
            Assert.That(new Matrix4x4Int(matrix1) == new Matrix4x4Int(matrix2), Is.EqualTo(result));
            Assert.That(new Matrix4x4Int(matrix2) == new Matrix4x4Int(matrix1), Is.EqualTo(result));
            Assert.That(new Matrix4x4Int(matrix1) != new Matrix4x4Int(matrix2), Is.EqualTo(!result));
            Assert.That(new Matrix4x4Int(matrix2) != new Matrix4x4Int(matrix1), Is.EqualTo(!result));
        });
    }
}
