using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
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

    private readonly KernelConstants<Constants> constants;

    private readonly Buffer2D<int> hostBuffer;
    private readonly Buffer2D<int> deviceBuffer;
    private DeviceMemory? hostMemory;
    private DeviceMemory? deviceMemory;
    private DeviceMemory? constantMemory;

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

    private static void KernelMethod(Span<int> result, Constants constants)
    {
        var width = constants.Width;
        var maxIter = constants.MaxIter;
        var spaceStart = constants.StartPoint;
        var spaceSize = new Vector2(1, 1) * constants.FractalSize;
        var globalInvocationId = GpuIntrinsic.GetGlobalInvocationId();
        var start = new Vector2Uint(globalInvocationId.X, globalInvocationId.Y) * workgroupSize;
        
        for (var wx = 0; wx < workgroupSize; ++wx)
        for (var wy = 0; wy < workgroupSize; ++wy)
        {
            var screenPoint = new Vector2(wx + start.X, wy + start.Y);
            var fractalSpacePoint = screenPoint * spaceSize / width + spaceStart;
            result[MapIndex(screenPoint, width)] = IterCount(fractalSpacePoint, maxIter);
        }
    }

    public GpuFractalGenerator(string appName)
    {
        factory = DeviceFactory.Create(BackendKind.Vulkan);
        factory.Init(new DeviceFactory.Desc(appName));

        device = factory.CreateDevice();
        device.Init(new ComputeDevice.Desc((factory.Adapters.FirstDiscreteOrNull() ?? factory.Adapters[0]).Id));

        hostBuffer = device.CreateBuffer2D<int>();
        deviceBuffer = device.CreateBuffer2D<int>();
        constants = device.CreateKernelConstants<Constants>();

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

        constants.Init("Constant buffer");
        constantMemory = constants.AllocateMemory("Constant memory", MemoryKindFlags.HostAndDeviceAccessible);
        constants.BindMemory(constantMemory);
        var constantValue = new Constants(maxIter, width, startPoint, fractalSize);
        constants.Set(constantValue);
        Debug.Assert(constantValue == constants.Get());

        using var compiler = factory.CreateKernelCompiler();
        compiler.Init(new KernelCompiler.Desc("Kernel compiler"));
        CompilerUtils.CompileKernel(KernelMethod, compiler, kernel, resourceBinding);

        resourceBinding.SetVariable(0, deviceBuffer);
        resourceBinding.SetVariable(1, constants);
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

    [StructLayout(LayoutKind.Sequential)]
    private readonly struct Constants
    {
        public readonly Vector2 StartPoint;
        public readonly int MaxIter;
        public readonly int Width;
        public readonly float FractalSize;

        public Constants(int maxIter, int width, Vector2 startPoint, float fractalSize)
        {
            MaxIter = maxIter;
            Width = width;
            StartPoint = startPoint;
            FractalSize = fractalSize;
        }

        public bool Equals(Constants other)
        {
            return StartPoint.Equals(other.StartPoint)
                   && MaxIter == other.MaxIter
                   && Width == other.Width
                   && FractalSize.Equals(other.FractalSize);
        }

        public override bool Equals(object? obj)
        {
            return obj is Constants other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(StartPoint, MaxIter, Width, FractalSize);
        }

        public static bool operator ==(Constants left, Constants right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Constants left, Constants right)
        {
            return !left.Equals(right);
        }
    }
}
