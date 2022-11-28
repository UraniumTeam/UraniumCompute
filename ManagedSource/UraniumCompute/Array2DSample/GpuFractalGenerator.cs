using System.Numerics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.ColorSpaces.Conversion;
using SixLabors.ImageSharp.PixelFormats;
using UraniumCompute.Acceleration;
using UraniumCompute.Backend;
using UraniumCompute.Compilation;
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

    private const string kernelSource = @"
RWStructuredBuffer<int> result : register(u0);

uint mapIndex(uint2 index)
{
    return index.x + index.y * IMG_WIDTH;
}

int IterCount(float2 c)
{
    float2 p = float2(0, 0);
    int iteration = 0;
    while (dot(p, p) <= 2*2 && iteration < MAX_ITER)
    {
        p = float2(p.x*p.x - p.y*p.y,  2*p.x*p.y) + c;
        iteration++;
    }

    return iteration;
}

[numthreads(1, 1, 1)]
void main(uint3 coord : SV_DispatchThreadID)
{
    float2 spaceStart = float2(START_POINT);
    float2 spaceSize  = float2(FRACTAL_SIZE);

    uint2 start = coord.xy * WORKGROUP_SIZE;
    for (uint wx = 0; wx < WORKGROUP_SIZE; ++wx)
    for (uint wy = 0; wy < WORKGROUP_SIZE; ++wy)
    {
        uint2 screenPoint = uint2(wx, wy) + start;
        float2 fractalSpacePoint = (float2)screenPoint * spaceSize / IMG_WIDTH + spaceStart;
        result[mapIndex(screenPoint)] = IterCount(fractalSpacePoint);
    }
}";

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
        using var bytecode = compiler.Compile(new KernelCompiler.Args(kernelSource, CompilerOptimizationLevel.Max, "main",
            new[]
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
