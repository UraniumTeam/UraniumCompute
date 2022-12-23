using System.Numerics;

namespace UraniumCompute.Common.Math;

public struct GpuRandom
{
    public int Seed;

    private const int RandomIa = 16807;
    private const int RandomIm = 2147483647;
    private const float RandomAm = 1.0f / RandomIm;
    private const float RandomIq = 127773u;
    private const int RandomIR = 2836;
    private const int RandomMask = 123459876;

    public Vector2 InsideUnitCircle()
    {
        var r = MathF.Sqrt(NextFloat());
        var theta = NextFloat() * 2 * MathF.PI;
        var x = r * MathF.Cos(theta);
        var y = r * MathF.Sin(theta);
        return new Vector2(x, y);
    }

    public Vector3 InsideUnitSphere()
    {
        var theta = NextFloat() * MathF.PI * 2;
        var phi = NextFloat() * MathF.PI;
        var r = NextFloat();
        var x = r * MathF.Sin(phi) * MathF.Cos(theta);
        var y = r * MathF.Sin(phi) * MathF.Sin(theta);
        var z = r * MathF.Cos(phi);
        return new Vector3(x, y, z);
    }

    public float NextFloat()
    {
        Cycle();
        return RandomAm * Seed;
    }

    public int NextInt()
    {
        Cycle();
        return Seed;
    }

    private void Cycle()
    {
        Seed ^= RandomMask;
        var k = (int)(Seed / RandomIq);
        Seed = (int)(RandomIa * (Seed - k * RandomIq) - RandomIR * k);

        if (Seed < 0)
        {
            Seed += RandomIm;
        }

        Seed ^= RandomMask;
    }

    public float NextFloat(float low, float high)
    {
        var v = NextFloat();
        return low * (1.0f - v) + high * v;
    }
}
