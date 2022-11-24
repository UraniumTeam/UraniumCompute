using UraniumCompute.Common;

namespace MathTests;

[TestFixture]
public class Vector4UintTests
{
    [Test]
    public void ReturnsComponents()
    {
        var vector = new Vector4Uint(1, 2, 3, 4);
        Assert.Multiple(() =>
        {
            Assert.That(vector.X, Is.EqualTo(1));
            Assert.That(vector.Y, Is.EqualTo(2));
            Assert.That(vector.Z, Is.EqualTo(3));
            Assert.That(vector.W, Is.EqualTo(4));
        });
        vector = new Vector4Uint(123);
        Assert.Multiple(() =>
        {
            Assert.That(vector.X, Is.EqualTo(123));
            Assert.That(vector.Y, Is.EqualTo(123));
            Assert.That(vector.Z, Is.EqualTo(123));
            Assert.That(vector.W, Is.EqualTo(123));
        });
    }

    [Test]
    public void DefaultValuesCorrect()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Vector4Uint.Zero, Is.EqualTo(new Vector4Uint(0)));
            Assert.That(Vector4Uint.One, Is.EqualTo(new Vector4Uint(1)));
        });

        Assert.Multiple(() =>
        {
            Assert.That(Vector4Uint.UnitX, Is.EqualTo(new Vector4Uint(1, 0, 0, 0)));
            Assert.That(Vector4Uint.UnitY, Is.EqualTo(new Vector4Uint(0, 1, 0, 0)));
            Assert.That(Vector4Uint.UnitZ, Is.EqualTo(new Vector4Uint(0, 0, 1, 0)));
            Assert.That(Vector4Uint.UnitW, Is.EqualTo(new Vector4Uint(0, 0, 0, 1)));
        });
    }

    [Test]
    public void ThrowsOnIncorrectLength()
    {
        Assert.Multiple(() =>
        {
            Assert.Catch<ArgumentException>(() => { _ = new Vector4Uint(Span<uint>.Empty); });
            Assert.Catch<ArgumentException>(() => { _ = new Vector4Uint(ReadOnlySpan<uint>.Empty); });
            Assert.Catch<ArgumentException>(() => { _ = new Vector4Uint(new uint[] { 1 }); });
            Assert.Catch<ArgumentException>(() => { _ = new Vector4Uint(new uint[] { 1, 2 }); });
            Assert.Catch<ArgumentException>(() => { _ = new Vector4Uint(new uint[] { 1, 2, 3 }); });
        });
    }

    [TestCase(new uint[] { 0, 0, 0, 0 }, new uint[] { 0, 0, 0, 0 }, true)]
    [TestCase(new uint[] { 1, 1, 1, 1 }, new uint[] { 1, 1, 1, 1 }, true)]
    [TestCase(new uint[] { 1, 2, 3, 4 }, new uint[] { 1, 2, 3, 4 }, true)]
    [TestCase(new uint[] { 1, 2, 3, 4 }, new uint[] { 0, 2, 3, 4 }, false)]
    [TestCase(new uint[] { 1, 2, 3, 4 }, new uint[] { 1, 0, 3, 4 }, false)]
    [TestCase(new uint[] { 1, 2, 3, 4 }, new uint[] { 1, 2, 0, 4 }, false)]
    [TestCase(new uint[] { 1, 2, 3, 4 }, new uint[] { 1, 2, 3, 0 }, false)]
    public void EqualityCheck(uint[] vector1, uint[] vector2, bool result)
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Vector4Uint(vector1).Equals(new Vector4Uint(vector2)), Is.EqualTo(result));
            Assert.That(new Vector4Uint(vector2).Equals(new Vector4Uint(vector1)), Is.EqualTo(result));
            Assert.That(new Vector4Uint(vector1) == new Vector4Uint(vector2), Is.EqualTo(result));
            Assert.That(new Vector4Uint(vector2) == new Vector4Uint(vector1), Is.EqualTo(result));
            Assert.That(new Vector4Uint(vector1) != new Vector4Uint(vector2), Is.EqualTo(!result));
            Assert.That(new Vector4Uint(vector2) != new Vector4Uint(vector1), Is.EqualTo(!result));
        });
    }

    [TestCase(new uint[] { 0, 0, 0, 0 }, new uint[] { 0, 0, 0, 0 }, new uint[] { 0, 0, 0, 0 })]
    [TestCase(new uint[] { 1, 1, 1, 1 }, new uint[] { 1, 1, 1, 1 }, new uint[] { 2, 2, 2, 2 })]
    public void Addition(uint[] vector1, uint[] vector2, uint[] result)
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Vector4Uint(vector1) + new Vector4Uint(vector2), Is.EqualTo(new Vector4Uint(result)));
            Assert.That(new Vector4Uint(vector2) + new Vector4Uint(vector1), Is.EqualTo(new Vector4Uint(result)));
        });
    }

    [TestCase(new uint[] { 0, 0, 0, 0 }, new uint[] { 0, 0, 0, 0 }, new uint[] { 0, 0, 0, 0 })]
    [TestCase(new uint[] { 1, 1, 1, 1 }, new uint[] { 1, 1, 1, 1 }, new uint[] { 0, 0, 0, 0 })]
    public void Subtraction(uint[] vector1, uint[] vector2, uint[] result)
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Vector4Uint(vector1) - new Vector4Uint(vector2), Is.EqualTo(new Vector4Uint(result)));
            Assert.That(new Vector4Uint(vector2) - new Vector4Uint(vector1), Is.EqualTo(-new Vector4Uint(result)));
        });
    }

    [TestCase(new uint[] { 1, 1, 1, 1 }, new uint[] { 1, 1, 1, 1 }, new uint[] { 1, 1, 1, 1 })]
    [TestCase(new uint[] { 4, 8, 16, 32 }, new uint[] { 2, 2, 2, 2 }, new uint[] { 8, 16, 32, 64 })]
    public void Multiplication(uint[] vector1, uint[] vector2, uint[] result)
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Vector4Uint(vector1) * new Vector4Uint(vector2), Is.EqualTo(new Vector4Uint(result)));
            Assert.That(new Vector4Uint(vector2) * new Vector4Uint(vector1), Is.EqualTo(new Vector4Uint(result)));
        });
    }

    [TestCase(new uint[] { 1, 1, 1, 1 }, new uint[] { 1, 1, 1, 1 }, new uint[] { 1, 1, 1, 1 })]
    [TestCase(new uint[] { 4, 8, 16, 32 }, new uint[] { 2, 2, 2, 2 }, new uint[] { 2, 4, 8, 16 })]
    [TestCase(new uint[] { 2, 11, 8, 15 }, new uint[] { 1, 2, 4, 3 }, new uint[] { 2, 5, 2, 5 })]
    public void Division(uint[] vector1, uint[] vector2, uint[] result)
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Vector4Uint(vector1) / new Vector4Uint(vector2), Is.EqualTo(new Vector4Uint(result)));
        });
    }

    [TestCase(new uint[] { 1, 1, 1, 1 }, (uint)1, new uint[] { 1, 1, 1, 1 })]
    [TestCase(new uint[] { 4, 4, 4, 4 }, (uint)2, new uint[] { 8, 8, 8, 8 })]
    public void ScalarMultiplication(uint[] vector1, uint scalar, uint[] result)
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Vector4Uint(vector1) * scalar, Is.EqualTo(new Vector4Uint(result)));
            Assert.That(scalar * new Vector4Uint(vector1), Is.EqualTo(new Vector4Uint(result)));
        });
    }
}
