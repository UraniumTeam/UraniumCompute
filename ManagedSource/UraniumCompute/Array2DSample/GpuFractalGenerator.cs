using System.Numerics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.ColorSpaces.Conversion;
using SixLabors.ImageSharp.PixelFormats;
using UraniumCompute.Acceleration;
using UraniumCompute.Backend;
using UraniumCompute.Common.Math;
using UraniumCompute.Compilation;
using UraniumCompute.Compiler.Decompiling;
using UraniumCompute.Compiler.InterimStructs;
using UraniumCompute.Utils;

namespace Array2DSample;

public sealed class GpuFractalGenerator : IFractalGenerator
{
    public Vector2 StartPoint { get; private set; }
    public float FractalSize { get; private set; }

    private int Width { get; set; }
    private int MaxIterations { get; set; }

    private readonly DeviceFactory factory;
    private readonly ComputeDevice device;

    private readonly Buffer2D<int> hostBuffer;
    private readonly Buffer2D<int> deviceBuffer;
    private DeviceMemory? hostMemory;
    private DeviceMemory? deviceMemory;

    private readonly ResourceBinding resourceBinding;
    private readonly Kernel kernel;
    private readonly CommandList commandList;

    private const int workgroupSize = 64;

    private static int MapIndex(Vector2 index, int width)
        => (int)(index.X + index.Y * width);

    private static int IterCount(Vector2 c, int maxIter)
    {
        var iteration = 0;
        var p = new Vector2(0, 0);
        var b = Vector2.Dot(p, p) <= 2 * 2;
        for (; iteration < maxIter && b; ++iteration)
        {
            p = c + new Vector2(p.X * p.X - p.Y * p.Y, 2 * p.X * p.Y);
            b = Vector2.Dot(p, p) <= 2 * 2;
        }

        return iteration;
    }

    private static string kernelSource = MethodCompilation.Compile((Span<int> result) =>
    {
        int width = 1024 * 2, maxIter = 64;
        uint workGrSize = 64;
        var spaceStart = new Vector2(-2.0f, -1.125f);
        var spaceSize = new Vector2(2.5f, 2.5f);
        var start = new Vector2Uint(GpuIntrinsic.GetGlobalInvocationId().X, GpuIntrinsic.GetGlobalInvocationId().Y)
                    * workGrSize;
        for (uint wx = 0; wx < workGrSize; ++wx)
        for (uint wy = 0; wy < workGrSize; ++wy)
        {
            var screenPoint = new Vector2(wx + start.X, wy + start.Y);
            var fractalSpacePoint = screenPoint * spaceSize / width + spaceStart;
            result[MapIndex(screenPoint, width)] = IterCount(fractalSpacePoint, maxIter);
        }
    });

    public GpuFractalGenerator(string appName)
    {
        factory = DeviceFactory.Create(BackendKind.Vulkan);
        factory.Init(new DeviceFactory.Desc(appName));

        device = factory.CreateDevice();
        device.Init(new ComputeDevice.Desc((factory.Adapters.FirstDiscreteOrNull() ?? factory.Adapters[0]).Id));

        hostBuffer = device.CreateBuffer2D<int>();
        deviceBuffer = device.CreateBuffer2D<int>();

        resourceBinding = device.CreateResourceBinding();
        kernel = device.CreateKernel();
        commandList = device.CreateCommandList();
        commandList.Init(new CommandList.Desc("Command list", HardwareQueueKindFlags.Compute));
    }

    public void Init(int maxIter, int width, Vector2 startPoint, float fractalSize)
    {
        Width = width;
        MaxIterations = maxIter;
        StartPoint = startPoint;
        FractalSize = fractalSize;

        hostBuffer.Init("Host buffer", (ulong)width, (ulong)width);
        hostMemory = hostBuffer.AllocateMemory("Host memory", MemoryKindFlags.HostAndDeviceAccessible);
        hostBuffer.BindMemory(hostMemory);

        deviceBuffer.Init("Device buffer", (ulong)width, (ulong)width);
        deviceMemory = deviceBuffer.AllocateMemory("Device memory", MemoryKindFlags.DeviceAccessible);
        deviceBuffer.BindMemory(deviceMemory);

        using var compiler = factory.CreateKernelCompiler();
        compiler.Init(new KernelCompiler.Desc("Kernel compiler"));
        using var bytecode = compiler.Compile(
            new KernelCompiler.Args(kernelSource, CompilerOptimizationLevel.Max, "main", new[]
            {
                new KernelCompiler.Define("WORKGROUP_SIZE", workgroupSize.ToString()),
                new KernelCompiler.Define("IMG_WIDTH", width.ToString()),
                new KernelCompiler.Define("MAX_ITER", maxIter.ToString()),
                new KernelCompiler.Define("START_POINT", $"{startPoint.X}, {startPoint.Y}"),
                new KernelCompiler.Define("FRACTAL_SIZE", $"{fractalSize}, {fractalSize}")
            }));

        resourceBinding.Init(new ResourceBinding.Desc("Resource binding", stackalloc[]
        {
            new KernelResourceDesc(0, KernelResourceKind.RWBuffer)
        }));

        resourceBinding.SetVariable(0, deviceBuffer);
        kernel.Init(new Kernel.Desc("Compute kernel", resourceBinding, bytecode[..]));
    }

    public void Render()
    {
        using (var cmd = commandList.Begin())
        {
            cmd.Dispatch(kernel, Width / workgroupSize, Width / workgroupSize, 1);
        }

        commandList.Submit();
        commandList.CompletionFence.WaitOnCpu();
    }

    public Image<Rgba32> GetResult()
    {
        commandList.ResetState();
        using (var cmd = commandList.Begin())
        {
            cmd.MemoryBarrier(deviceBuffer, AccessFlags.KernelWrite, AccessFlags.TransferRead);
            cmd.Copy(deviceBuffer, hostBuffer);
            cmd.MemoryBarrier(hostBuffer, AccessFlags.TransferWrite, AccessFlags.HostRead);
        }

        commandList.Submit();
        commandList.CompletionFence.WaitOnCpu();

        var image = new Image<Rgba32>(Width, Width);
        var converter = new ColorSpaceConverter();
        using (var map = hostBuffer.Map())
        {
            for (var x = 0; x < Width; ++x)
            for (var y = 0; y < Width; ++y)
            {
                var c = Math.Min(byte.MaxValue, map[x, y]);
                image[x, y] = converter.ToRgb(FractalPlotFacts.GetPointColor(MaxIterations, c));
            }
        }

        return image;
    }

    public void Dispose()
    {
        factory.Dispose();
        device.Dispose();
        hostBuffer.Dispose();
        deviceBuffer.Dispose();
        hostMemory?.Dispose();
        deviceMemory?.Dispose();
        resourceBinding.Dispose();
        kernel.Dispose();
        commandList.Dispose();
    }
}
