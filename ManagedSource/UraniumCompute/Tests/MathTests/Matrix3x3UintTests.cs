using UraniumCompute.Common.Math;

namespace MathTests;

[TestFixture]
public class Matrix3x3UintTests
{
    [TestCase(
        new uint[]
        {
            1, 2, 3,
            1, 2, 3,
            1, 2, 3
        },
        new uint[]
        {
            1, 2, 3,
            1, 2, 3,
            1, 2, 3
        },
        new uint[]
        {
            2, 4, 6,
            2, 4, 6,
            2, 4, 6
        })]
    public void Addition(uint[] matrix1, uint[] matrix2, uint[] result)
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Matrix3x3Uint(matrix1) + new Matrix3x3Uint(matrix2), Is.EqualTo(new Matrix3x3Uint(result)));
            Assert.That(new Matrix3x3Uint(matrix2) + new Matrix3x3Uint(matrix1), Is.EqualTo(new Matrix3x3Uint(result)));
        });
    }

    [TestCase(
        new uint[]
        {
            1, 2, 3,
            1, 2, 3,
            1, 2, 3
        },
        new uint[]
        {
            1, 2, 3,
            1, 2, 3,
            1, 2, 3
        },
        new uint[]
        {
            0, 0, 0,
            0, 0, 0,
            0, 0, 0
        })]
    [TestCase(
        new uint[]
        {
            1, 8, 3,
            4, 2, 3,
            1, 6, 9
        },
        new uint[]
        {
            0, 5, 3,
            4, 2, 3,
            1, 2, 3
        },
        new uint[]
        {
            1, 3, 0,
            0, 0, 0,
            0, 4, 6
        })]
    public void Subtraction(uint[] matrix1, uint[] matrix2, uint[] result)
    {
        var matrix = new Matrix3x3Uint(matrix1) - new Matrix3x3Uint(matrix2);
        Assert.That(matrix, Is.EqualTo(new Matrix3x3Uint(result)));
    }

    [TestCase(
        new uint[]
        {
            1, 2, 3,
            1, 2, 3,
            1, 2, 3
        },
        new uint[]
        {
            1, 2, 3,
            1, 2, 3,
            1, 2, 3
        },
        new uint[]
        {
            6, 12, 18,
            6, 12, 18,
            6, 12, 18
        })]
    [TestCase(
        new uint[]
        {
            1, 8, 3,
            4, 2, 3,
            1, 6, 9
        },
        new uint[]
        {
            2, 5, 3,
            4, 2, 3,
            1, 2, 3
        },
        new uint[]
        {
            31, 27, 36,
            19, 30, 27,
            35, 35, 48
        })]
    public void Multiplication(uint[] matrix1, uint[] matrix2, uint[] result)
    {
        Assert.That(new Matrix3x3Uint(matrix1) * new Matrix3x3Uint(matrix2),
            Is.EqualTo(new Matrix3x3Uint(result)));
    }

    [TestCase(
        new uint[]
        {
            1, 2, 3,
            1, 2, 3,
            1, 2, 3
        },
        2u,
        new uint[]
        {
            2, 4, 6,
            2, 4, 6,
            2, 4, 6
        })]
    public void ScalarMultiplication(uint[] matrix1, uint scalar, uint[] result)
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Matrix3x3Uint(matrix1) * scalar, Is.EqualTo(new Matrix3x3Uint(result)));
            Assert.That(scalar * new Matrix3x3Uint(matrix1), Is.EqualTo(new Matrix3x3Uint(result)));
        });
    }

    [TestCase(
        new uint[]
        {
            1, 2, 3,
            1, 2, 3,
            1, 2, 3
        },
        new uint[]
        {
            1, 1, 1,
            2, 2, 2,
            3, 3, 3
        })]
    public void Transposition(uint[] matrix, uint[] result)
    {
        Assert.Multiple(() =>
        {
            Assert.That(Matrix3x3Uint.Transpose(new Matrix3x3Uint(matrix)), Is.EqualTo(new Matrix3x3Uint(result)));
            Assert.That(Matrix3x3Uint.Transpose(Matrix3x3Uint.Transpose(new Matrix3x3Uint(matrix))),
                Is.EqualTo(new Matrix3x3Uint(matrix)));
        });
    }

    [TestCase(
        new uint[]
        {
            0, 0, 0,
            0, 0, 0,
            0, 0, 0
        },
        new uint[]
        {
            0, 0, 0,
            0, 0, 0,
            0, 0, 0
        },
        true)]
    [TestCase(
        new uint[]
        {
            1, 2, 3,
            1, 2, 3,
            1, 2, 3
        },
        new uint[]
        {
            1, 2, 3,
            1, 2, 3,
            1, 2, 3
        },
        true)]
    [TestCase(
        new uint[]
        {
            0, 2, 3,
            1, 2, 3,
            1, 2, 3
        },
        new uint[]
        {
            1, 2, 3,
            1, 2, 3,
            1, 2, 3
        },
        false)]
    [TestCase(
        new uint[]
        {
            1, 2, 3,
            1, 0, 3,
            1, 2, 3
        },
        new uint[]
        {
            1, 2, 3,
            1, 2, 3,
            1, 2, 3
        },
        false)]
    [TestCase(
        new uint[]
        {
            1, 2, 3,
            1, 2, 3,
            1, 2, 0
        },
        new uint[]
        {
            1, 2, 3,
            1, 2, 3,
            1, 2, 3
        },
        false)]
    public void EqualityCheck(uint[] matrix1, uint[] matrix2, bool result)
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Matrix3x3Uint(matrix1).Equals(new Matrix3x3Uint(matrix2)), Is.EqualTo(result));
            Assert.That(new Matrix3x3Uint(matrix2).Equals(new Matrix3x3Uint(matrix1)), Is.EqualTo(result));
            Assert.That(new Matrix3x3Uint(matrix1) == new Matrix3x3Uint(matrix2), Is.EqualTo(result));
            Assert.That(new Matrix3x3Uint(matrix2) == new Matrix3x3Uint(matrix1), Is.EqualTo(result));
            Assert.That(new Matrix3x3Uint(matrix1) != new Matrix3x3Uint(matrix2), Is.EqualTo(!result));
            Assert.That(new Matrix3x3Uint(matrix2) != new Matrix3x3Uint(matrix1), Is.EqualTo(!result));
        });
    }
}
