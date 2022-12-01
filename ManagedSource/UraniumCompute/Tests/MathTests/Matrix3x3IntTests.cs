using UraniumCompute.Common.Math;

namespace MathTests;

[TestFixture]
public class Matrix3x3IntTests
{
    [TestCase(
        new[]
        {
            1, 2, 3,
            1, 2, 3,
            1, 2, 3
        },
        new[]
        {
            1, 2, 3,
            1, 2, 3,
            1, 2, 3
        },
        new[]
        {
            2, 4, 6,
            2, 4, 6,
            2, 4, 6
        })]
    public void Addition(int[] matrix1, int[] matrix2, int[] result)
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Matrix3x3Int(matrix1) + new Matrix3x3Int(matrix2), Is.EqualTo(new Matrix3x3Int(result)));
            Assert.That(new Matrix3x3Int(matrix2) + new Matrix3x3Int(matrix1), Is.EqualTo(new Matrix3x3Int(result)));
        });
    }

    [TestCase(
        new[]
        {
            1, 2, 3,
            1, 2, 3,
            1, 2, 3
        },
        new[]
        {
            1, 2, 3,
            1, 2, 3,
            1, 2, 3
        },
        new[]
        {
            0, 0, 0,
            0, 0, 0,
            0, 0, 0
        })]
    [TestCase(
        new[]
        {
            1, 8, 3,
            4, -2, 3,
            -1, 6, 9
        },
        new[]
        {
            2, 5, 3,
            4, 2, 3,
            -1, 2, 3
        },
        new[]
        {
            -1, 3, 0,
            0, -4, 0,
            -2, 4, 6
        })]
    public void Subtraction(int[] matrix1, int[] matrix2, int[] result)
    {
        var matrix = new Matrix3x3Int(matrix1) - new Matrix3x3Int(matrix2);
        Assert.That(matrix, Is.EqualTo(new Matrix3x3Int(result)));
    }

    [TestCase(
        new[]
        {
            1, 2, 3,
            1, 2, 3,
            1, 2, 3
        },
        new[]
        {
            -1, -2, -3,
            -1, -2, -3,
            -1, -2, -3
        })]
    public void Negation(int[] matrix1, int[] result)
    {
        Assert.That(-new Matrix3x3Int(matrix1), Is.EqualTo(new Matrix3x3Int(result)));
    }

    [TestCase(
        new[]
        {
            1, 2, 3,
            1, 2, 3,
            1, 2, 3
        },
        new[]
        {
            1, 2, 3,
            1, 2, 3,
            1, 2, 3
        },
        new[]
        {
            6, 12, 18,
            6, 12, 18,
            6, 12, 18
        })]
    [TestCase(
        new[]
        {
            1, 8, 3,
            4, -2, 3,
            -1, 6, 9
        },
        new[]
        {
            2, 5, 3,
            4, 2, 3,
            -1, 2, 3
        },
        new[]
        {
            31, 27, 36,
            -3, 22, 15,
            13, 25, 42
        })]
    public void Multiplication(int[] matrix1, int[] matrix2, int[] result)
    {
        Assert.That(new Matrix3x3Int(matrix1) * new Matrix3x3Int(matrix2),
            Is.EqualTo(new Matrix3x3Int(result)));
    }

    [TestCase(
        new[]
        {
            1, 2, 3,
            1, 2, 3,
            1, 2, 3
        },
        2,
        new[]
        {
            2, 4, 6,
            2, 4, 6,
            2, 4, 6
        })]
    public void ScalarMultiplication(int[] matrix1, int scalar, int[] result)
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Matrix3x3Int(matrix1) * scalar, Is.EqualTo(new Matrix3x3Int(result)));
            Assert.That(scalar * new Matrix3x3Int(matrix1), Is.EqualTo(new Matrix3x3Int(result)));
        });
    }

    [TestCase(
        new[]
        {
            1, 2, 3,
            1, 2, 3,
            1, 2, 3
        },
        new[]
        {
            1, 1, 1,
            2, 2, 2,
            3, 3, 3
        })]
    public void Transposition(int[] matrix, int[] result)
    {
        Assert.Multiple(() =>
        {
            Assert.That(Matrix3x3Int.Transpose(new Matrix3x3Int(matrix)), Is.EqualTo(new Matrix3x3Int(result)));
            Assert.That(Matrix3x3Int.Transpose(Matrix3x3Int.Transpose(new Matrix3x3Int(matrix))),
                Is.EqualTo(new Matrix3x3Int(matrix)));
        });
    }

    [TestCase(
        new[]
        {
            0, 0, 0,
            0, 0, 0,
            0, 0, 0
        },
        new[]
        {
            0, 0, 0,
            0, 0, 0,
            0, 0, 0
        },
        true)]
    [TestCase(
        new[]
        {
            1, 2, 3,
            1, 2, 3,
            1, 2, 3
        },
        new[]
        {
            1, 2, 3,
            1, 2, 3,
            1, 2, 3
        },
        true)]
    [TestCase(
        new[]
        {
            0, 2, 3,
            1, 2, 3,
            1, 2, 3
        },
        new[]
        {
            1, 2, 3,
            1, 2, 3,
            1, 2, 3
        },
        false)]
    [TestCase(
        new[]
        {
            1, 2, 3,
            1, 0, 3,
            1, 2, 3
        },
        new[]
        {
            1, 2, 3,
            1, 2, 3,
            1, 2, 3
        },
        false)]
    [TestCase(
        new[]
        {
            1, 2, 3,
            1, 2, 3,
            1, 2, 0
        },
        new[]
        {
            1, 2, 3,
            1, 2, 3,
            1, 2, 3
        },
        false)]
    public void EqualityCheck(int[] matrix1, int[] matrix2, bool result)
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Matrix3x3Int(matrix1).Equals(new Matrix3x3Int(matrix2)), Is.EqualTo(result));
            Assert.That(new Matrix3x3Int(matrix2).Equals(new Matrix3x3Int(matrix1)), Is.EqualTo(result));
            Assert.That(new Matrix3x3Int(matrix1) == new Matrix3x3Int(matrix2), Is.EqualTo(result));
            Assert.That(new Matrix3x3Int(matrix2) == new Matrix3x3Int(matrix1), Is.EqualTo(result));
            Assert.That(new Matrix3x3Int(matrix1) != new Matrix3x3Int(matrix2), Is.EqualTo(!result));
            Assert.That(new Matrix3x3Int(matrix2) != new Matrix3x3Int(matrix1), Is.EqualTo(!result));
        });
    }
}
