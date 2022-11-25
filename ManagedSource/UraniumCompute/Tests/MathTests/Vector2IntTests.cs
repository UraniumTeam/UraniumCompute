using UraniumCompute.Common;
using UraniumCompute.Common.Math;

namespace MathTests;

[TestFixture]
public class Vector2IntTests
{
    [Test]
    public void ReturnsComponents()
    {
        var vector = new Vector2Int(1, 2);
        Assert.Multiple(() =>
        {
            Assert.That(vector.X, Is.EqualTo(1));
            Assert.That(vector.Y, Is.EqualTo(2));
        });
        vector = new Vector2Int(123);
        Assert.Multiple(() =>
        {
            Assert.That(vector.X, Is.EqualTo(123));
            Assert.That(vector.Y, Is.EqualTo(123));
        });
    }

    [Test]
    public void DefaultValuesCorrect()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Vector2Int.Zero, Is.EqualTo(new Vector2Int(0)));
            Assert.That(Vector2Int.One, Is.EqualTo(new Vector2Int(1)));
        });

        Assert.Multiple(() =>
        {
            Assert.That(Vector2Int.UnitX, Is.EqualTo(new Vector2Int(1, 0)));
            Assert.That(Vector2Int.UnitY, Is.EqualTo(new Vector2Int(0, 1)));
        });
    }

    [Test]
    public void ThrowsOnIncorrectLength()
    {
        Assert.Multiple(() =>
        {
            Assert.Catch<ArgumentException>(() => { _ = new Vector2Int(Span<int>.Empty); });
            Assert.Catch<ArgumentException>(() => { _ = new Vector2Int(ReadOnlySpan<int>.Empty); });
            Assert.Catch<ArgumentException>(() => { _ = new Vector2Int(new[] { 1 }); });
        });
    }

    [TestCase(new[] { 0, 0 }, new[] { 0, 0 }, true)]
    [TestCase(new[] { 1, 1 }, new[] { 1, 1 }, true)]
    [TestCase(new[] { 1, 2 }, new[] { 1, 2 }, true)]
    [TestCase(new[] { 1, 2 }, new[] { 0, 2 }, false)]
    [TestCase(new[] { 1, 2 }, new[] { 1, 0 }, false)]
    public void EqualityCheck(int[] vector1, int[] vector2, bool result)
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Vector2Int(vector1).Equals(new Vector2Int(vector2)), Is.EqualTo(result));
            Assert.That(new Vector2Int(vector2).Equals(new Vector2Int(vector1)), Is.EqualTo(result));
            Assert.That(new Vector2Int(vector1) == new Vector2Int(vector2), Is.EqualTo(result));
            Assert.That(new Vector2Int(vector2) == new Vector2Int(vector1), Is.EqualTo(result));
            Assert.That(new Vector2Int(vector1) != new Vector2Int(vector2), Is.EqualTo(!result));
            Assert.That(new Vector2Int(vector2) != new Vector2Int(vector1), Is.EqualTo(!result));
        });
    }

    [TestCase(new[] { 0, 0 }, new[] { 0, 0 }, new[] { 0, 0 })]
    [TestCase(new[] { 1, 1 }, new[] { 1, 1 }, new[] { 2, 2 })]
    [TestCase(new[] { 1, 2 }, new[] { -3, 5 }, new[] { -2, 7 })]
    public void Addition(int[] vector1, int[] vector2, int[] result)
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Vector2Int(vector1) + new Vector2Int(vector2), Is.EqualTo(new Vector2Int(result)));
            Assert.That(new Vector2Int(vector2) + new Vector2Int(vector1), Is.EqualTo(new Vector2Int(result)));
        });
    }

    [TestCase(new[] { 0, 0 }, new[] { 0, 0 }, new[] { 0, 0 })]
    [TestCase(new[] { 1, 1 }, new[] { 1, 1 }, new[] { 0, 0 })]
    [TestCase(new[] { 1, 2 }, new[] { 3, -5 }, new[] { -2, 7 })]
    public void Subtraction(int[] vector1, int[] vector2, int[] result)
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Vector2Int(vector1) - new Vector2Int(vector2), Is.EqualTo(new Vector2Int(result)));
            Assert.That(new Vector2Int(vector2) - new Vector2Int(vector1), Is.EqualTo(-new Vector2Int(result)));
        });
    }

    [TestCase(new[] { 1, 1 }, new[] { 1, 1 }, new[] { 1, 1 })]
    [TestCase(new[] { 4, 4 }, new[] { 2, 2 }, new[] { 8, 8 })]
    [TestCase(new[] { 2, 11 }, new[] { 1, 2 }, new[] { 2, 22 })]
    public void Multiplication(int[] vector1, int[] vector2, int[] result)
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Vector2Int(vector1) * new Vector2Int(vector2), Is.EqualTo(new Vector2Int(result)));
            Assert.That(new Vector2Int(vector2) * new Vector2Int(vector1), Is.EqualTo(new Vector2Int(result)));
        });
    }

    [TestCase(new[] { 1, 1 }, new[] { 1, 1 }, new[] { 1, 1 })]
    [TestCase(new[] { 4, 4 }, new[] { 2, 2 }, new[] { 2, 2 })]
    [TestCase(new[] { 2, 11 }, new[] { 1, 2 }, new[] { 2, 5 })]
    public void Division(int[] vector1, int[] vector2, int[] result)
    {
        Assert.Multiple(() =>
        {
            var vector2Int = new Vector2Int(vector1) / new Vector2Int(vector2);
            var expected = new Vector2Int(result);
            Assert.That(vector2Int, Is.EqualTo(expected));
        });
    }

    [TestCase(new[] { 1, 1 }, 1, new[] { 1, 1 })]
    [TestCase(new[] { 4, 4 }, 2, new[] { 8, 8 })]
    [TestCase(new[] { 2, 11 }, -3, new[] { -6, -33 })]
    public void ScalarMultiplication(int[] vector1, int scalar, int[] result)
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Vector2Int(vector1) * scalar, Is.EqualTo(new Vector2Int(result)));
            Assert.That(scalar * new Vector2Int(vector1), Is.EqualTo(new Vector2Int(result)));
        });
    }
}
