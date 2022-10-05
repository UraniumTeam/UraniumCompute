using UraniumCompute.Acceleration;
using UraniumCompute.Backend;
using UraniumCompute.Compilation;
using UraniumCompute.Utils;

using var factory = DeviceFactory.Create(BackendKind.Vulkan);
factory.Init(new DeviceFactory.Desc("Array transformation sample"));

using var device = factory.CreateDevice();
device.Init(new ComputeDevice.Desc(factory.Adapters.FirstDiscrete().Id));

using var hostBuffer = device.CreateBuffer<uint>();
hostBuffer.Init("Host buffer", 1024 * 1024);
using var hostMemory = hostBuffer.AllocateMemory("Host memory", MemoryKindFlags.HostAndDeviceAccessible);
hostBuffer.BindMemory(hostMemory);

using (var map = hostBuffer.Map())
{
    for (var i = 0; i < map.Count; i++)
    {
        map[i] = (uint)i;
    }
}

using var deviceBuffer = device.CreateBuffer<uint>();
deviceBuffer.Init("Device buffer", hostBuffer.LongCount);
using var deviceMemory = deviceBuffer.AllocateMemory("Device memory", MemoryKindFlags.DeviceAccessible);
deviceBuffer.BindMemory(deviceMemory);

const string kernelSource = @"
RWStructuredBuffer<uint> values : register(u0);

uint fib(uint n)
{
    if(n <= 1) return n;
    n %= 16;

    uint c = 1;
    uint p = 1;

    for (uint i = 2; i < n; ++i)
    {
        uint t = c;
        c += p;
        p = t;
    }

    return c;
}

[numthreads(1, 1, 1)]
void main(uint3 globalInvocationID : SV_DispatchThreadID)
{
    uint i = globalInvocationID.x;
    values[i] = fib(values[i]);
}";

using var compiler = factory.CreateKernelCompiler();
compiler.Init(new KernelCompiler.Desc("Kernel compiler"));
using var bytecode = compiler.Compile(new KernelCompiler.Args(kernelSource, CompilerOptimizationLevel.Max, "main"));

using var resourceBinding = device.CreateResourceBinding();
resourceBinding.Init(new ResourceBinding.Desc("Resource binding", new[]
{
    new KernelResourceDesc(0, KernelResourceKind.RWBuffer)
}));

resourceBinding.SetVariable(0, deviceBuffer);

using var kernel = device.CreateKernel();
kernel.Init(new Kernel.Desc("Compute kernel", resourceBinding, bytecode));

using var commandList = device.CreateCommandList();
commandList.Init(new CommandList.Desc("Command list", HardwareQueueKindFlags.Compute));

using (var cmd = commandList.Begin())
{
    cmd.Copy(hostBuffer, deviceBuffer);
    cmd.MemoryBarrier(deviceBuffer, AccessFlags.KernelWrite, AccessFlags.KernelRead);
    cmd.Dispatch(kernel, deviceBuffer.Count, 1, 1);
    cmd.MemoryBarrier(deviceBuffer, AccessFlags.KernelWrite, AccessFlags.TransferRead);
    cmd.Copy(deviceBuffer, hostBuffer);
    cmd.MemoryBarrier(hostBuffer, AccessFlags.TransferWrite, AccessFlags.HostRead);
}

commandList.Submit();
commandList.CompletionFence.WaitOnCpu();

using (var map = hostBuffer.Map())
{
    Console.WriteLine($"Calculation results: [{string.Join(", ", map.Select(x => x.ToString()).Take(32))}, ...]");
}
