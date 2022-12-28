using System.Numerics;
using UraniumCompute.Common.Math;

namespace MathTests;

[TestFixture]
public sealed class RandomTests
{
    [Test]
    public void TestUnitCircle()
    {
        var random = new GpuRandom();
        var previous = Vector2.One;
        for (var i = 0; i < 100_000; i++)
        {
            var v = random.InsideUnitCircle();
            Assert.That(Vector2.Distance(v, Vector2.Zero), Is.LessThanOrEqualTo(1.001f));
            Assert.That(v, Is.Not.EqualTo(previous));
            previous = v;
        }
    }

    [Test]
    public void TestUnitSphere()
    {
        var random = new GpuRandom();
        var previous = Vector3.One;
        for (var i = 0; i < 100_000; i++)
        {
            var v = random.InsideUnitSphere();
            Assert.That(Vector3.Distance(v, Vector3.Zero), Is.LessThanOrEqualTo(1.001f));
            Assert.That(v, Is.Not.EqualTo(previous));
            previous = v;
        }
    }
}
