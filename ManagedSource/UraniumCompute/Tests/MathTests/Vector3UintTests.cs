using UraniumCompute.Common;

namespace MathTests;

[TestFixture]
public class Vector3UintTests
{
    [Test]
    public void ReturnsComponents()
    {
        var vector = new Vector3Uint(1, 2, 3);
        Assert.Multiple(() =>
        {
            Assert.That(vector.X, Is.EqualTo(1));
            Assert.That(vector.Y, Is.EqualTo(2));
            Assert.That(vector.Z, Is.EqualTo(3));
        });
        vector = new Vector3Uint(123);
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
            Assert.That(Vector3Uint.Zero, Is.EqualTo(new Vector3Uint(0)));
            Assert.That(Vector3Uint.One, Is.EqualTo(new Vector3Uint(1)));
        });

        Assert.Multiple(() =>
        {
            Assert.That(Vector3Uint.UnitX, Is.EqualTo(new Vector3Uint(1, 0, 0)));
            Assert.That(Vector3Uint.UnitY, Is.EqualTo(new Vector3Uint(0, 1, 0)));
            Assert.That(Vector3Uint.UnitZ, Is.EqualTo(new Vector3Uint(0, 0, 1)));
        });
    }

    [Test]
    public void ThrowsOnIncorrectLength()
    {
        Assert.Multiple(() =>
        {
            Assert.Catch<ArgumentException>(() => { _ = new Vector3Uint(Span<uint>.Empty); });
            Assert.Catch<ArgumentException>(() => { _ = new Vector3Uint(ReadOnlySpan<uint>.Empty); });
            Assert.Catch<ArgumentException>(() => { _ = new Vector3Uint(new uint[] { 1 }); });
            Assert.Catch<ArgumentException>(() => { _ = new Vector3Uint(new uint[] { 1, 2 }); });
        });
    }

    [TestCase(new uint[] { 0, 0, 0 }, new uint[] { 0, 0, 0 }, true)]
    [TestCase(new uint[] { 1, 1, 1 }, new uint[] { 1, 1, 1 }, true)]
    [TestCase(new uint[] { 1, 2, 3 }, new uint[] { 1, 2, 3 }, true)]
    [TestCase(new uint[] { 1, 2, 3 }, new uint[] { 0, 2, 3 }, false)]
    [TestCase(new uint[] { 1, 2, 3 }, new uint[] { 1, 0, 3 }, false)]
    [TestCase(new uint[] { 1, 2, 3 }, new uint[] { 1, 2, 0 }, false)]
    public void EqualityCheck(uint[] vector1, uint[] vector2, bool result)
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Vector3Uint(vector1).Equals(new Vector3Uint(vector2)), Is.EqualTo(result));
            Assert.That(new Vector3Uint(vector2).Equals(new Vector3Uint(vector1)), Is.EqualTo(result));
            Assert.That(new Vector3Uint(vector1) == new Vector3Uint(vector2), Is.EqualTo(result));
            Assert.That(new Vector3Uint(vector2) == new Vector3Uint(vector1), Is.EqualTo(result));
            Assert.That(new Vector3Uint(vector1) != new Vector3Uint(vector2), Is.EqualTo(!result));
            Assert.That(new Vector3Uint(vector2) != new Vector3Uint(vector1), Is.EqualTo(!result));
        });
    }

    [TestCase(new uint[] { 0, 0, 0 }, new uint[] { 0, 0, 0 }, new uint[] { 0, 0, 0 })]
    [TestCase(new uint[] { 1, 1, 1 }, new uint[] { 1, 1, 1 }, new uint[] { 2, 2, 2 })]
    [TestCase(new uint[] { 3, 5, 10 }, new uint[] { 1, 2, 3 }, new uint[] { 4, 7, 13 })]
    public void Addition(uint[] vector1, uint[] vector2, uint[] result)
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Vector3Uint(vector1) + new Vector3Uint(vector2), Is.EqualTo(new Vector3Uint(result)));
            Assert.That(new Vector3Uint(vector2) + new Vector3Uint(vector1), Is.EqualTo(new Vector3Uint(result)));
        });
    }

    [TestCase(new uint[] { 0, 0, 0 }, new uint[] { 0, 0, 0 }, new uint[] { 0, 0, 0 })]
    [TestCase(new uint[] { 1, 1, 1 }, new uint[] { 1, 1, 1 }, new uint[] { 0, 0, 0 })]
    [TestCase(new uint[] { 3, 5, 10 }, new uint[] { 1, 2, 3}, new uint[] { 2, 3, 7 })]
    public void Subtraction(uint[] vector1, uint[] vector2, uint[] result)
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Vector3Uint(vector1) - new Vector3Uint(vector2), Is.EqualTo(new Vector3Uint(result)));
            Assert.That(new Vector3Uint(vector2) - new Vector3Uint(vector1), Is.EqualTo(-new Vector3Uint(result)));
        });
    }

    [TestCase(new uint[] { 1, 1, 1 }, new uint[] { 1, 1, 1 }, new uint[] { 1, 1, 1 })]
    [TestCase(new uint[] { 4, 4, 4 }, new uint[] { 2, 2, 2 }, new uint[] { 8, 8, 8 })]
    [TestCase(new uint[] { 2, 11, 8 }, new uint[] { 1, 2, 4 }, new uint[] { 2, 22, 32 })]
    public void Multiplication(uint[] vector1, uint[] vector2, uint[] result)
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Vector3Uint(vector1) * new Vector3Uint(vector2), Is.EqualTo(new Vector3Uint(result)));
            Assert.That(new Vector3Uint(vector2) * new Vector3Uint(vector1), Is.EqualTo(new Vector3Uint(result)));
        });
    }

    [TestCase(new uint[] { 1, 1, 1 }, new uint[] { 1, 1, 1 }, new uint[] { 1, 1, 1 })]
    [TestCase(new uint[] { 4, 4, 4 }, new uint[] { 2, 2, 2 }, new uint[] { 2, 2, 2 })]
    [TestCase(new uint[] { 2, 11, 8 }, new uint[] { 1, 2, 4 }, new uint[] { 2, 5, 2 })]
    public void Division(uint[] vector1, uint[] vector2, uint[] result)
    {
        Assert.Multiple(() =>
        {
            var vector3Uint = new Vector3Uint(vector1) / new Vector3Uint(vector2);
            var expected = new Vector3Uint(result);
            Assert.That(vector3Uint, Is.EqualTo(expected));
        });
    }

    [TestCase(new uint[] { 1, 1, 1 }, (uint)1, new uint[] { 1, 1, 1 })]
    [TestCase(new uint[] { 4, 4, 4 }, (uint)2, new uint[] { 8, 8, 8 })]
    [TestCase(new uint[] { 2, 11, 8 }, (uint)3, new uint[] { 6, 33, 24 })]
    public void ScalarMultiplication(uint[] vector1, uint scalar, uint[] result)
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Vector3Uint(vector1) * scalar, Is.EqualTo(new Vector3Uint(result)));
            Assert.That(scalar * new Vector3Uint(vector1), Is.EqualTo(new Vector3Uint(result)));
        });
    }
}
