using System.Numerics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Array2DSample;

public interface IFractalGenerator : IDisposable
{
    public float FractalSize { get; }
    void Init(int maxIter, int width, Vector2 startPoint, float fractalSize);
    void Render();
    Image<Rgba32> GetResult();
}
