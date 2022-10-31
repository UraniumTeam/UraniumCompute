using System.Diagnostics;
using System.Numerics;
using Array2DSample;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;

const float fullFractalSize = 2.5f;

const int maxIter = 64;
const int imageScale = 8;
const int width = imageScale * 1024;

// using IFractalGenerator generator = new CpuFractalGenerator();
using IFractalGenerator generator = new GpuFractalGenerator("Mandelbrot Set Sample");

// generator.Init(maxIter, width, new Vector2(-2.0f, -1.125f), fullFractalSize);
generator.Init(maxIter, width, new Vector2(-0.56f, 0.57f), 0.03f);

var fractalScale = fullFractalSize / generator.FractalSize;
var text = $"UraniumCompute v0.1\nMandelbrot Set {width} x {width} px\nFractal scale: {fractalScale:F2}x";

var sw = new Stopwatch();
sw.Start();
Console.WriteLine(text);
Console.WriteLine(new string('=', 128));
generator.Render();
Console.WriteLine($"It took {sw.ElapsedMilliseconds}ms to render the fractal");

using var image = generator.GetResult();

var collection = new FontCollection();
var family = collection.Add("Roboto-Bold.ttf");
var font = family.CreateFont(imageScale * 22, FontStyle.Regular);
var rect = new RectangleF(PointF.Empty, new SizeF(400, 100) * imageScale);

image.Mutate(context => context
    .Fill(Color.White, rect)
    .Draw(Color.Black, imageScale, rect)
    .DrawText(text, font, Color.Black, new PointF(20, 2) * imageScale)
    .Resize(new Size(2048, 2048)));

image.Save("../../fractal.png");
