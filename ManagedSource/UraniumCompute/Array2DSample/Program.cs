using SixLabors.ImageSharp;
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.ColorSpaces.Conversion;
using SixLabors.ImageSharp.PixelFormats;
using UraniumCompute.Acceleration;
using UraniumCompute.Backend;
using UraniumCompute.Compilation;
using UraniumCompute.Utils;

using var factory = DeviceFactory.Create(BackendKind.Vulkan);
factory.Init(new DeviceFactory.Desc("Mandelbrot Set sample"));

using var device = factory.CreateDevice();
device.Init(new ComputeDevice.Desc((factory.Adapters.FirstDiscreteOrNull() ?? factory.Adapters[0]).Id));

const int width = 4 * 1024;
const int workgroupSize = 64;
const int maxIter = 255;

using var hostBuffer = device.CreateBuffer2D<int>();
hostBuffer.Init("Host buffer", width, width);
using var hostMemory = hostBuffer.AllocateMemory("Host memory", MemoryKindFlags.HostAndDeviceAccessible);
hostBuffer.BindMemory(hostMemory);

using var deviceBuffer = device.CreateBuffer2D<int>();
deviceBuffer.Init("Device buffer", width, width);
using var deviceMemory = deviceBuffer.AllocateMemory("Device memory", MemoryKindFlags.DeviceAccessible);
deviceBuffer.BindMemory(deviceMemory);

const string kernelSource = @"
RWStructuredBuffer<int> result : register(u0);

uint mapIndex(uint2 index)
{
    return index.x + index.y * IMG_WIDTH;
}

int IterCount(float2 c)
{
    float x = 0.0;
    float y = 0.0;
    int iteration = 0;
    while (x*x + y*y <= 2*2 && iteration < MAX_ITER)
    {
        float xtemp = x*x - y*y + c.x;
        y = 2*x*y + c.y;
        x = xtemp;
        iteration++;
    }

    return iteration;
}

[numthreads(1, 1, 1)]
void main(uint3 coord : SV_DispatchThreadID)
{
    float2 spaceStart = float2(-2.0, -1.12);
    float2 spaceSize  = float2(3.47, 2.24);
    // float2 spaceStart = float2(-0.56, 0.57);
    // float2 spaceSize  = float2(0.03, 0.03);

    uint2 start = coord.xy * WORKGROUP_SIZE;
    for (uint wx = 0; wx < WORKGROUP_SIZE; ++wx)
    for (uint wy = 0; wy < WORKGROUP_SIZE; ++wy)
    {
        uint2 p = uint2(wx, wy) + start;
        float2 c = float2(
                (float)p.x * spaceSize.x / IMG_WIDTH + spaceStart.x,
                (float)p.y * spaceSize.y / IMG_WIDTH + spaceStart.y
        );
        result[mapIndex(p)] = IterCount(c);
    }
}";

using var compiler = factory.CreateKernelCompiler();
compiler.Init(new KernelCompiler.Desc("Kernel compiler"));
using var bytecode = compiler.Compile(new KernelCompiler.Args(kernelSource, CompilerOptimizationLevel.Max, "main",
    new[]
    {
        new KernelCompiler.Define("WORKGROUP_SIZE", workgroupSize.ToString()),
        new KernelCompiler.Define("IMG_WIDTH", width.ToString()),
        new KernelCompiler.Define("MAX_ITER", maxIter.ToString())
    }));

using var resourceBinding = device.CreateResourceBinding();
resourceBinding.Init(new ResourceBinding.Desc("Resource binding", stackalloc[]
{
    new KernelResourceDesc(0, KernelResourceKind.RWBuffer)
}));

resourceBinding.SetVariable(0, deviceBuffer);

using var kernel = device.CreateKernel();
kernel.Init(new Kernel.Desc("Compute kernel", resourceBinding, bytecode[..]));

using var commandList = device.CreateCommandList();
commandList.Init(new CommandList.Desc("Command list", HardwareQueueKindFlags.Compute));

using (var cmd = commandList.Begin())
{
    cmd.Dispatch(kernel, width / workgroupSize, width / workgroupSize, 1);
    cmd.MemoryBarrier(deviceBuffer, AccessFlags.KernelWrite, AccessFlags.TransferRead);
    cmd.Copy(deviceBuffer, hostBuffer);
    cmd.MemoryBarrier(hostBuffer, AccessFlags.TransferWrite, AccessFlags.HostRead);
}

commandList.Submit();
commandList.CompletionFence.WaitOnCpu();

using var image = new Image<Rgba32>(width, width);
var converter = new ColorSpaceConverter();
using (var map = hostBuffer.Map())
{
    for (var x = 0; x < width; ++x)
    for (var y = 0; y < width; ++y)
    {
        var c = Math.Min(255, map[x, y]);
        var hue = (int)(255f * c / maxIter);
        var saturation = 255;
        var value = c < maxIter ? 255 : 0;
        image[x, y] = converter.ToRgb(new Hsv(hue, saturation, value));
    }
}

image.Save("fractal.png");
