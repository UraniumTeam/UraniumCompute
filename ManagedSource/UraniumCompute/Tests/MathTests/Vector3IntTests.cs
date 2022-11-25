using UraniumCompute.Common;
using UraniumCompute.Common.Math;

namespace MathTests;

[TestFixture]
public class Vector3IntTests
{
    [Test]
    public void ReturnsComponents()
    {
        var vector = new Vector3Int(1, 2, 3);
        Assert.Multiple(() =>
        {
            Assert.That(vector.X, Is.EqualTo(1));
            Assert.That(vector.Y, Is.EqualTo(2));
            Assert.That(vector.Z, Is.EqualTo(3));
        });
        vector = new Vector3Int(123);
        Assert.Multiple(() =>
        {
            Assert.That(vector.X, Is.EqualTo(123));
            Assert.That(vector.Y, Is.EqualTo(123));
            Assert.That(vector.Z, Is.EqualTo(123));
        });
    }

    [Test]
    public void DefaultValuesCorrect()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Vector3Int.Zero, Is.EqualTo(new Vector3Int(0)));
            Assert.That(Vector3Int.One, Is.EqualTo(new Vector3Int(1)));
        });

        Assert.Multiple(() =>
        {
            Assert.That(Vector3Int.UnitX, Is.EqualTo(new Vector3Int(1, 0, 0)));
            Assert.That(Vector3Int.UnitY, Is.EqualTo(new Vector3Int(0, 1, 0)));
            Assert.That(Vector3Int.UnitZ, Is.EqualTo(new Vector3Int(0, 0, 1)));
        });
    }

    [Test]
    public void ThrowsOnIncorrectLength()
    {
        Assert.Multiple(() =>
        {
            Assert.Catch<ArgumentException>(() => { _ = new Vector3Int(Span<int>.Empty); });
            Assert.Catch<ArgumentException>(() => { _ = new Vector3Int(ReadOnlySpan<int>.Empty); });
            Assert.Catch<ArgumentException>(() => { _ = new Vector3Int(new[] { 1 }); });
            Assert.Catch<ArgumentException>(() => { _ = new Vector3Int(new[] { 1, 2 }); });
        });
    }

    [TestCase(new[] { 0, 0, 0 }, new[] { 0, 0, 0 }, true)]
    [TestCase(new[] { 1, 1, 1 }, new[] { 1, 1, 1 }, true)]
    [TestCase(new[] { 1, 2, 3 }, new[] { 1, 2, 3 }, true)]
    [TestCase(new[] { 1, 2, 3 }, new[] { 0, 2, 3 }, false)]
    [TestCase(new[] { 1, 2, 3 }, new[] { 1, 0, 3 }, false)]
    [TestCase(new[] { 1, 2, 3 }, new[] { 1, 2, 0 }, false)]
    public void EqualityCheck(int[] vector1, int[] vector2, bool result)
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Vector3Int(vector1).Equals(new Vector3Int(vector2)), Is.EqualTo(result));
            Assert.That(new Vector3Int(vector2).Equals(new Vector3Int(vector1)), Is.EqualTo(result));
            Assert.That(new Vector3Int(vector1) == new Vector3Int(vector2), Is.EqualTo(result));
            Assert.That(new Vector3Int(vector2) == new Vector3Int(vector1), Is.EqualTo(result));
            Assert.That(new Vector3Int(vector1) != new Vector3Int(vector2), Is.EqualTo(!result));
            Assert.That(new Vector3Int(vector2) != new Vector3Int(vector1), Is.EqualTo(!result));
        });
    }

    [TestCase(new[] { 0, 0, 0 }, new[] { 0, 0, 0 }, new[] { 0, 0, 0 })]
    [TestCase(new[] { 1, 1, 1 }, new[] { 1, 1, 1 }, new[] { 2, 2, 2 })]
    [TestCase(new[] { 1, 2, 3 }, new[] { -3, 5, 10 }, new[] { -2, 7, 13 })]
    public void Addition(int[] vector1, int[] vector2, int[] result)
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Vector3Int(vector1) + new Vector3Int(vector2), Is.EqualTo(new Vector3Int(result)));
            Assert.That(new Vector3Int(vector2) + new Vector3Int(vector1), Is.EqualTo(new Vector3Int(result)));
        });
    }

    [TestCase(new[] { 0, 0, 0 }, new[] { 0, 0, 0 }, new[] { 0, 0, 0 })]
    [TestCase(new[] { 1, 1, 1 }, new[] { 1, 1, 1 }, new[] { 0, 0, 0 })]
    [TestCase(new[] { 1, 2, 3 }, new[] { 3, -5, -10 }, new[] { -2, 7, 13 })]
    public void Subtraction(int[] vector1, int[] vector2, int[] result)
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Vector3Int(vector1) - new Vector3Int(vector2), Is.EqualTo(new Vector3Int(result)));
            Assert.That(new Vector3Int(vector2) - new Vector3Int(vector1), Is.EqualTo(-new Vector3Int(result)));
        });
    }

    [TestCase(new[] { 1, 1, 1 }, new[] { 1, 1, 1 }, new[] { 1, 1, 1 })]
    [TestCase(new[] { 4, 4, 4 }, new[] { 2, 2, 2 }, new[] { 8, 8, 8 })]
    [TestCase(new[] { 2, 11, -8 }, new[] { 1, 2, -4 }, new[] { 2, 22, 32 })]
    public void Multiplication(int[] vector1, int[] vector2, int[] result)
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Vector3Int(vector1) * new Vector3Int(vector2), Is.EqualTo(new Vector3Int(result)));
            Assert.That(new Vector3Int(vector2) * new Vector3Int(vector1), Is.EqualTo(new Vector3Int(result)));
        });
    }

    [TestCase(new[] { 1, 1, 1 }, new[] { 1, 1, 1 }, new[] { 1, 1, 1 })]
    [TestCase(new[] { 4, 4, 4 }, new[] { 2, 2, 2 }, new[] { 2, 2, 2 })]
    [TestCase(new[] { 2, 11, 8 }, new[] { 1, 2, 4 }, new[] { 2, 5, 2 })]
    public void Division(int[] vector1, int[] vector2, int[] result)
    {
        Assert.Multiple(() =>
        {
            var vector3Int = new Vector3Int(vector1) / new Vector3Int(vector2);
            var expected = new Vector3Int(result);
            Assert.That(vector3Int, Is.EqualTo(expected));
        });
    }

    [TestCase(new[] { 1, 1, 1 }, 1, new[] { 1, 1, 1 })]
    [TestCase(new[] { 4, 4, 4 }, 2, new[] { 8, 8, 8 })]
    [TestCase(new[] { 2, 11, -8 }, -3, new[] { -6, -33, 24 })]
    public void ScalarMultiplication(int[] vector1, int scalar, int[] result)
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Vector3Int(vector1) * scalar, Is.EqualTo(new Vector3Int(result)));
            Assert.That(scalar * new Vector3Int(vector1), Is.EqualTo(new Vector3Int(result)));
        });
    }
}
