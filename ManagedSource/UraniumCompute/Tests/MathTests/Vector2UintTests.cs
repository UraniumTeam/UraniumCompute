using UraniumCompute.Common;

namespace MathTests;

[TestFixture]
public class Vector2UintTests
{
    [Test]
    public void ReturnsComponents()
    {
        var vector = new Vector2Uint(1, 2);
        Assert.Multiple(() =>
        {
            Assert.That(vector.X, Is.EqualTo(1));
            Assert.That(vector.Y, Is.EqualTo(2));
        });
        vector = new Vector2Uint(123);
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
            Assert.That(Vector2Uint.Zero, Is.EqualTo(new Vector2Uint(0)));
            Assert.That(Vector2Uint.One, Is.EqualTo(new Vector2Uint(1)));
        });

        Assert.Multiple(() =>
        {
            Assert.That(Vector2Uint.UnitX, Is.EqualTo(new Vector2Uint(1, 0)));
            Assert.That(Vector2Uint.UnitY, Is.EqualTo(new Vector2Uint(0, 1)));
        });
    }

    [Test]
    public void ThrowsOnIncorrectLength()
    {
        Assert.Multiple(() =>
        {
            Assert.Catch<ArgumentException>(() => { _ = new Vector2Uint(Span<uint>.Empty); });
            Assert.Catch<ArgumentException>(() => { _ = new Vector2Uint(ReadOnlySpan<uint>.Empty); });
            Assert.Catch<ArgumentException>(() => { _ = new Vector2Uint(new uint[] { 1 }); });
        });
    }

    [TestCase(new uint[] { 0, 0 }, new uint[] { 0, 0 }, true)]
    [TestCase(new uint[] { 1, 1 }, new uint[] { 1, 1 }, true)]
    [TestCase(new uint[] { 1, 2 }, new uint[] { 1, 2 }, true)]
    [TestCase(new uint[] { 1, 2 }, new uint[] { 0, 2 }, false)]
    [TestCase(new uint[] { 1, 2 }, new uint[] { 1, 0 }, false)]
    public void EqualityCheck(uint[] vector1, uint[] vector2, bool result)
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Vector2Uint(vector1).Equals(new Vector2Uint(vector2)), Is.EqualTo(result));
            Assert.That(new Vector2Uint(vector2).Equals(new Vector2Uint(vector1)), Is.EqualTo(result));
            Assert.That(new Vector2Uint(vector1) == new Vector2Uint(vector2), Is.EqualTo(result));
            Assert.That(new Vector2Uint(vector2) == new Vector2Uint(vector1), Is.EqualTo(result));
            Assert.That(new Vector2Uint(vector1) != new Vector2Uint(vector2), Is.EqualTo(!result));
            Assert.That(new Vector2Uint(vector2) != new Vector2Uint(vector1), Is.EqualTo(!result));
        });
    }

    [TestCase(new uint[] { 0, 0 }, new uint[] { 0, 0 }, new uint[] { 0, 0 })]
    [TestCase(new uint[] { 1, 1 }, new uint[] { 1, 1 }, new uint[] { 2, 2 })]
    [TestCase(new uint[] { 3, 5 }, new uint[] { 1, 2 }, new uint[] { 4, 7 })]
    public void Addition(uint[] vector1, uint[] vector2, uint[] result)
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Vector2Uint(vector1) + new Vector2Uint(vector2), Is.EqualTo(new Vector2Uint(result)));
            Assert.That(new Vector2Uint(vector2) + new Vector2Uint(vector1), Is.EqualTo(new Vector2Uint(result)));
        });
    }

    [TestCase(new uint[] { 0, 0 }, new uint[] { 0, 0 }, new uint[] { 0, 0 })]
    [TestCase(new uint[] { 1, 1 }, new uint[] { 1, 1 }, new uint[] { 0, 0 })]
    [TestCase(new uint[] { 3, 5 }, new uint[] { 1, 2 }, new uint[] { 2, 3 })]
    public void Subtraction(uint[] vector1, uint[] vector2, uint[] result)
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Vector2Uint(vector1) - new Vector2Uint(vector2), Is.EqualTo(new Vector2Uint(result)));
            Assert.That(new Vector2Uint(vector2) - new Vector2Uint(vector1), Is.EqualTo(-new Vector2Uint(result)));
        });
    }

    [TestCase(new uint[] { 1, 1 }, new uint[] { 1, 1 }, new uint[] { 1, 1 })]
    [TestCase(new uint[] { 4, 4 }, new uint[] { 2, 2 }, new uint[] { 8, 8 })]
    [TestCase(new uint[] { 2, 11 }, new uint[] { 1, 2 }, new uint[] { 2, 22 })]
    public void Multiplication(uint[] vector1, uint[] vector2, uint[] result)
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Vector2Uint(vector1) * new Vector2Uint(vector2), Is.EqualTo(new Vector2Uint(result)));
            Assert.That(new Vector2Uint(vector2) * new Vector2Uint(vector1), Is.EqualTo(new Vector2Uint(result)));
        });
    }

    [TestCase(new uint[] { 1, 1 }, new uint[] { 1, 1 }, new uint[] { 1, 1 })]
    [TestCase(new uint[] { 4, 4 }, new uint[] { 2, 2 }, new uint[] { 2, 2 })]
    [TestCase(new uint[] { 2, 11 }, new uint[] { 1, 2 }, new uint[] { 2, 5 })]
    public void Division(uint[] vector1, uint[] vector2, uint[] result)
    {
        Assert.Multiple(() =>
        {
            var vector2Uint = new Vector2Uint(vector1) / new Vector2Uint(vector2);
            var expected = new Vector2Uint(result);
            Assert.That(vector2Uint, Is.EqualTo(expected));
        });
    }

    [TestCase(new uint[] { 4, 4 }, (uint)0, new uint[] { 0, 0 })]
    [TestCase(new uint[] { 1, 1 }, (uint)1, new uint[] { 1, 1 })]
    [TestCase(new uint[] { 4, 4 }, (uint)2, new uint[] { 8, 8 })]
    [TestCase(new uint[] { 2, 11 }, (uint)3, new uint[] { 6, 33 })]
    public void ScalarMultiplication(uint[] vector1, uint scalar, uint[] result)
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Vector2Uint(vector1) * scalar, Is.EqualTo(new Vector2Uint(result)));
            Assert.That(scalar * new Vector2Uint(vector1), Is.EqualTo(new Vector2Uint(result)));
        });
    }
}
