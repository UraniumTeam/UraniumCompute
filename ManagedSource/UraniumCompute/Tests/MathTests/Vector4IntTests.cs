using UraniumCompute.Common;
using UraniumCompute.Common.Math;

namespace MathTests;

[TestFixture]
public class Vector4IntTests
{
    [Test]
    public void ReturnsComponents()
    {
        var vector = new Vector4Int(1, 2, 3, 4);
        Assert.Multiple(() =>
        {
            Assert.That(vector.X, Is.EqualTo(1));
            Assert.That(vector.Y, Is.EqualTo(2));
            Assert.That(vector.Z, Is.EqualTo(3));
            Assert.That(vector.W, Is.EqualTo(4));
        });
        vector = new Vector4Int(123);
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
            Assert.That(Vector4Int.Zero, Is.EqualTo(new Vector4Int(0)));
            Assert.That(Vector4Int.One, Is.EqualTo(new Vector4Int(1)));
        });

        Assert.Multiple(() =>
        {
            Assert.That(Vector4Int.UnitX, Is.EqualTo(new Vector4Int(1, 0, 0, 0)));
            Assert.That(Vector4Int.UnitY, Is.EqualTo(new Vector4Int(0, 1, 0, 0)));
            Assert.That(Vector4Int.UnitZ, Is.EqualTo(new Vector4Int(0, 0, 1, 0)));
            Assert.That(Vector4Int.UnitW, Is.EqualTo(new Vector4Int(0, 0, 0, 1)));
        });
    }

    [Test]
    public void ThrowsOnIncorrectLength()
    {
        Assert.Multiple(() =>
        {
            Assert.Catch<ArgumentException>(() => { _ = new Vector4Int(Span<int>.Empty); });
            Assert.Catch<ArgumentException>(() => { _ = new Vector4Int(ReadOnlySpan<int>.Empty); });
            Assert.Catch<ArgumentException>(() => { _ = new Vector4Int(new[] { 1 }); });
            Assert.Catch<ArgumentException>(() => { _ = new Vector4Int(new[] { 1, 2 }); });
            Assert.Catch<ArgumentException>(() => { _ = new Vector4Int(new[] { 1, 2, 3 }); });
        });
    }

    [TestCase(new[] { 0, 0, 0, 0 }, new[] { 0, 0, 0, 0 }, true)]
    [TestCase(new[] { 1, 1, 1, 1 }, new[] { 1, 1, 1, 1 }, true)]
    [TestCase(new[] { 1, 2, 3, 4 }, new[] { 1, 2, 3, 4 }, true)]
    [TestCase(new[] { 1, 2, 3, 4 }, new[] { 0, 2, 3, 4 }, false)]
    [TestCase(new[] { 1, 2, 3, 4 }, new[] { 1, 0, 3, 4 }, false)]
    [TestCase(new[] { 1, 2, 3, 4 }, new[] { 1, 2, 0, 4 }, false)]
    [TestCase(new[] { 1, 2, 3, 4 }, new[] { 1, 2, 3, 0 }, false)]
    public void EqualityCheck(int[] vector1, int[] vector2, bool result)
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Vector4Int(vector1).Equals(new Vector4Int(vector2)), Is.EqualTo(result));
            Assert.That(new Vector4Int(vector2).Equals(new Vector4Int(vector1)), Is.EqualTo(result));
            Assert.That(new Vector4Int(vector1) == new Vector4Int(vector2), Is.EqualTo(result));
            Assert.That(new Vector4Int(vector2) == new Vector4Int(vector1), Is.EqualTo(result));
            Assert.That(new Vector4Int(vector1) != new Vector4Int(vector2), Is.EqualTo(!result));
            Assert.That(new Vector4Int(vector2) != new Vector4Int(vector1), Is.EqualTo(!result));
        });
    }

    [TestCase(new[] { 0, 0, 0, 0 }, new[] { 0, 0, 0, 0 }, new[] { 0, 0, 0, 0 })]
    [TestCase(new[] { 1, 1, 1, 1 }, new[] { 1, 1, 1, 1 }, new[] { 2, 2, 2, 2 })]
    [TestCase(new[] { 1, 2, 3, 4 }, new[] { -3, 5, 10, 0 }, new[] { -2, 7, 13, 4 })]
    public void Addition(int[] vector1, int[] vector2, int[] result)
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Vector4Int(vector1) + new Vector4Int(vector2), Is.EqualTo(new Vector4Int(result)));
            Assert.That(new Vector4Int(vector2) + new Vector4Int(vector1), Is.EqualTo(new Vector4Int(result)));
        });
    }

    [TestCase(new[] { 0, 0, 0, 0 }, new[] { 0, 0, 0, 0 }, new[] { 0, 0, 0, 0 })]
    [TestCase(new[] { 1, 1, 1, 1 }, new[] { 1, 1, 1, 1 }, new[] { 0, 0, 0, 0 })]
    [TestCase(new[] { 1, 2, 3, 4 }, new[] { 3, -5, -10, 0 }, new[] { -2, 7, 13, 4 })]
    public void Subtraction(int[] vector1, int[] vector2, int[] result)
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Vector4Int(vector1) - new Vector4Int(vector2), Is.EqualTo(new Vector4Int(result)));
            Assert.That(new Vector4Int(vector2) - new Vector4Int(vector1), Is.EqualTo(-new Vector4Int(result)));
        });
    }

    [TestCase(new[] { 1, 1, 1, 1 }, new[] { 1, 1, 1, 1 }, new[] { 1, 1, 1, 1 })]
    [TestCase(new[] { 4, 4, 4, 4 }, new[] { 2, 2, 2, 2 }, new[] { 8, 8, 8, 8 })]
    [TestCase(new[] { 2, 11, -8, 15 }, new[] { 1, 2, -4, -3 }, new[] { 2, 22, 32, -45 })]
    public void Multiplication(int[] vector1, int[] vector2, int[] result)
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Vector4Int(vector1) * new Vector4Int(vector2), Is.EqualTo(new Vector4Int(result)));
            Assert.That(new Vector4Int(vector2) * new Vector4Int(vector1), Is.EqualTo(new Vector4Int(result)));
        });
    }

    [TestCase(new[] { 1, 1, 1, 1 }, new[] { 1, 1, 1, 1 }, new[] { 1, 1, 1, 1 })]
    [TestCase(new[] { 4, 4, 4, 4 }, new[] { 2, 2, 2, 2 }, new[] { 2, 2, 2, 2 })]
    [TestCase(new[] { 2, 11, 8, 15 }, new[] { 1, 2, 4, 3 }, new[] { 2, 5, 2, 5 })]
    public void Division(int[] vector1, int[] vector2, int[] result)
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Vector4Int(vector1) / new Vector4Int(vector2), Is.EqualTo(new Vector4Int(result)));
        });
    }

    [TestCase(new[] { 1, 1, 1, 1 }, 1, new[] { 1, 1, 1, 1 })]
    [TestCase(new[] { 4, 4, 4, 4 }, 2, new[] { 8, 8, 8, 8 })]
    [TestCase(new[] { 2, 11, -8, 15 }, -3, new[] { -6, -33, 24, -45 })]
    public void ScalarMultiplication(int[] vector1, int scalar, int[] result)
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Vector4Int(vector1) * scalar, Is.EqualTo(new Vector4Int(result)));
            Assert.That(scalar * new Vector4Int(vector1), Is.EqualTo(new Vector4Int(result)));
        });
    }
}
