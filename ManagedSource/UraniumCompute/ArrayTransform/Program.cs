using UraniumCompute.Acceleration;
using UraniumCompute.Backend;
using UraniumCompute.Compilation;
using UraniumCompute.Compiler.InterimStructs;
using UraniumCompute.Utils;

using var factory = DeviceFactory.Create(BackendKind.Vulkan);
factory.Init(new DeviceFactory.Desc("Array transformation sample"));

using var device = factory.CreateDevice();
device.Init(new ComputeDevice.Desc((factory.Adapters.FirstDiscreteOrNull() ?? factory.Adapters[0]).Id));

using var hostBuffer = device.CreateBuffer1D<uint>();
hostBuffer.Init("Host buffer", 256 * 1024);
using var hostMemory = hostBuffer.AllocateMemory("Host memory", MemoryKindFlags.HostAndDeviceAccessible);
hostBuffer.BindMemory(hostMemory);

using (var map = hostBuffer.Map())
{
    var i = 0u;
    foreach (ref var x in map[..])
    {
        x = i++;
    }
}

using var deviceBuffer = device.CreateBuffer1D<uint>();
deviceBuffer.Init("Device buffer", hostBuffer.LongCount);
using var deviceMemory = deviceBuffer.AllocateMemory("Device memory", MemoryKindFlags.DeviceAccessible);
deviceBuffer.BindMemory(deviceMemory);

const int workgroupSize = 128;

static uint Fib(uint n)
{
    n %= 16;
    if (n <= 1)
    {
        return n;
    }

    uint c = 1;
    uint p = 1;

    for (uint i = 2; i < n; ++i)
    {
        var t = c;
        c += p;
        p = t;
    }

    return c;
}

using var compiler = factory.CreateKernelCompiler();
compiler.Init(new KernelCompiler.Desc("Kernel compiler"));
using var resourceBinding = device.CreateResourceBinding();
using var kernel = device.CreateKernel();
CompilerUtils.CompileKernel((Span<uint> values) =>
{
    var index = (int)GpuIntrinsic.GetGlobalInvocationId().X;
    for (var i = index * workgroupSize; i < (index + 1) * workgroupSize; ++i)
    {
        values[i] = Fib(values[i]);
    }
}, compiler, kernel, resourceBinding);

resourceBinding.SetVariable(0, deviceBuffer);

using var commandList = device.CreateCommandList();
commandList.Init(new CommandList.Desc("Command list", HardwareQueueKindFlags.Compute));

using (var cmd = commandList.Begin())
{
    cmd.Copy(hostBuffer, deviceBuffer);
    cmd.MemoryBarrier(deviceBuffer, AccessFlags.TransferWrite, AccessFlags.KernelRead);
    cmd.Dispatch(kernel, deviceBuffer.Count / workgroupSize, 1, 1);
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
