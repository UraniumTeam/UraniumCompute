using UraniumCompute.Common.Math;

namespace MathTests;

[TestFixture]
public class Matrix4x4UintTests
{
    [TestCase(
        new uint[]
        {
            1, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 4
        },
        new uint[]
        {
            1, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 4
        },
        new uint[]
        {
            2, 4, 6, 8,
            2, 4, 6, 8,
            2, 4, 6, 8,
            2, 4, 6, 8
        })]
    public void Addition(uint[] matrix1, uint[] matrix2, uint[] result)
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Matrix4x4Uint(matrix1) + new Matrix4x4Uint(matrix2), Is.EqualTo(new Matrix4x4Uint(result)));
            Assert.That(new Matrix4x4Uint(matrix2) + new Matrix4x4Uint(matrix1), Is.EqualTo(new Matrix4x4Uint(result)));
        });
    }

    [TestCase(
        new uint[]
        {
            1, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 4
        },
        new uint[]
        {
            1, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 4
        },
        new uint[]
        {
            0, 0, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0
        })]
    [TestCase(
        new uint[]
        {
            4, 4, 4, 4,
            4, 4, 4, 4,
            4, 4, 4, 4,
            4, 4, 4, 4
        },
        new uint[]
        {
            1, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 4
        },
        new uint[]
        {
            3, 2, 1, 0,
            3, 2, 1, 0,
            3, 2, 1, 0,
            3, 2, 1, 0
        })]
    public void Subtraction(uint[] matrix1, uint[] matrix2, uint[] result)
    {
        var matrix = new Matrix4x4Uint(matrix1) - new Matrix4x4Uint(matrix2);
        Assert.That(matrix, Is.EqualTo(new Matrix4x4Uint(result)));
    }

    [TestCase(
        new uint[]
        {
            1, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 4
        },
        new uint[]
        {
            1, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 4
        },
        new uint[]
        {
            10, 20, 30, 40,
            10, 20, 30, 40,
            10, 20, 30, 40,
            10, 20, 30, 40
        })]
    public void Multiplication(uint[] matrix1, uint[] matrix2, uint[] result)
    {
        Assert.That(new Matrix4x4Uint(matrix1) * new Matrix4x4Uint(matrix2), Is.EqualTo(new Matrix4x4Uint(result)));
    }

    [TestCase(
        new uint[]
        {
            1, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 4
        },
        2u,
        new uint[]
        {
            2, 4, 6, 8,
            2, 4, 6, 8,
            2, 4, 6, 8,
            2, 4, 6, 8
        })]
    [TestCase(
        new uint[]
        {
            1, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 4
        },
        200000000u,
        new uint[]
        {
            200000000, 400000000, 600000000, 800000000,
            200000000, 400000000, 600000000, 800000000,
            200000000, 400000000, 600000000, 800000000,
            200000000, 400000000, 600000000, 800000000
        })]
    public void ScalarMultiplication(uint[] matrix1, uint scalar, uint[] result)
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Matrix4x4Uint(matrix1) * scalar, Is.EqualTo(new Matrix4x4Uint(result)));
            Assert.That(scalar * new Matrix4x4Uint(matrix1), Is.EqualTo(new Matrix4x4Uint(result)));
        });
    }

    [TestCase(
        new uint[]
        {
            1, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 4
        },
        new uint[]
        {
            1, 1, 1, 1,
            2, 2, 2, 2,
            3, 3, 3, 3,
            4, 4, 4, 4
        })]
    public void Transposition(uint[] matrix, uint[] result)
    {
        Assert.Multiple(() =>
        {
            Assert.That(Matrix4x4Uint.Transpose(new Matrix4x4Uint(matrix)), Is.EqualTo(new Matrix4x4Uint(result)));
            Assert.That(Matrix4x4Uint.Transpose(Matrix4x4Uint.Transpose(new Matrix4x4Uint(matrix))),
                Is.EqualTo(new Matrix4x4Uint(matrix)));
        });
    }

    [TestCase(
        new uint[]
        {
            0, 0, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0
        },
        new uint[]
        {
            0, 0, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0
        },
        true)]
    [TestCase(
        new uint[]
        {
            1, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 4
        },
        new uint[]
        {
            1, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 4
        },
        true)]
    [TestCase(
        new uint[]
        {
            0, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 4
        },
        new uint[]
        {
            1, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 4
        },
        false)]
    [TestCase(
        new uint[]
        {
            1, 2, 3, 4,
            1, 0, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 4
        },
        new uint[]
        {
            1, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 4
        },
        false)]
    [TestCase(
        new uint[]
        {
            1, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 0, 4,
            1, 2, 3, 4
        },
        new uint[]
        {
            1, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 4
        },
        false)]
    [TestCase(
        new uint[]
        {
            1, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 0
        },
        new uint[]
        {
            1, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 4,
            1, 2, 3, 4
        },
        false)]
    public void EqualityCheck(uint[] matrix1, uint[] matrix2, bool result)
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Matrix4x4Uint(matrix1).Equals(new Matrix4x4Uint(matrix2)), Is.EqualTo(result));
            Assert.That(new Matrix4x4Uint(matrix2).Equals(new Matrix4x4Uint(matrix1)), Is.EqualTo(result));
            Assert.That(new Matrix4x4Uint(matrix1) == new Matrix4x4Uint(matrix2), Is.EqualTo(result));
            Assert.That(new Matrix4x4Uint(matrix2) == new Matrix4x4Uint(matrix1), Is.EqualTo(result));
            Assert.That(new Matrix4x4Uint(matrix1) != new Matrix4x4Uint(matrix2), Is.EqualTo(!result));
            Assert.That(new Matrix4x4Uint(matrix2) != new Matrix4x4Uint(matrix1), Is.EqualTo(!result));
        });
    }
}
