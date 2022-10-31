using System.Numerics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.ColorSpaces.Conversion;
using SixLabors.ImageSharp.PixelFormats;

namespace Array2DSample;

public sealed class CpuFractalGenerator : IFractalGenerator
{
    public Vector2 StartPoint { get; private set; }
    public float FractalSize { get; private set; }

    private int Width { get; set; }
    private int MaxIterations { get; set; }

    private Image<Rgba32> result;

    public void Init(int maxIter, int width, Vector2 startPoint, float fractalSize)
    {
        result = new Image<Rgba32>(width, width);
        MaxIterations = maxIter;
        FractalSize = fractalSize;
        StartPoint = startPoint;
        Width = width;
    }

    public void Render()
    {
        var converter = new ColorSpaceConverter();
        for (var x = 0; x < Width; ++x)
        for (var y = 0; y < Width; ++y)
        {
            var screenPoint = new Vector2(x, y);
            var fractalSpacePoint = screenPoint * FractalSize / Width + StartPoint;
            result[x, y] =
                converter.ToRgb(FractalPlotFacts.GetPointColor(MaxIterations, IterCount(fractalSpacePoint, MaxIterations)));
        }
    }

    public Image<Rgba32> GetResult()
    {
        return result;
    }

    public void Dispose()
    {
    }

    private static int IterCount(Vector2 c, int maxIter)
    {
        var p = new Vector2(0, 0);
        var iteration = 0;
        while (p.LengthSquared() <= 2 * 2 && iteration < maxIter)
        {
            p = new Vector2(p.X * p.X - p.Y * p.Y, 2 * p.X * p.Y) + c;
            iteration++;
        }

        return iteration;
    }
}
