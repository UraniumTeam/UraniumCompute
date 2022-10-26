using SixLabors.ImageSharp.ColorSpaces;

namespace Array2DSample;

public static class FractalPlotFacts
{
    public static Hsv GetPointColor(int maxIter, int iterCount)
    {
        var hue = (int)(255f * iterCount / maxIter);
        return new Hsv(hue, 255, iterCount < maxIter ? 255 : 0);
    }
}
