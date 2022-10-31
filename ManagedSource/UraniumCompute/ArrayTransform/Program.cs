﻿using UraniumCompute.Acceleration;
using UraniumCompute.Backend;
using UraniumCompute.Compilation;
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
const string kernelSource = @"
RWStructuredBuffer<uint> values : register(u0);

uint fib(uint n)
{
    n %= 16;
    if(n <= 1) return n;

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
    uint index = globalInvocationID.x;
    for (uint i = index * WORKGROUP_SIZE; i < (index + 1) * WORKGROUP_SIZE; ++i)
        values[i] = fib(values[i]);
}";

using var compiler = factory.CreateKernelCompiler();
compiler.Init(new KernelCompiler.Desc("Kernel compiler"));
using var bytecode = compiler.Compile(new KernelCompiler.Args(kernelSource, CompilerOptimizationLevel.Max, "main",
    new[] { new KernelCompiler.Define("WORKGROUP_SIZE", workgroupSize.ToString()) }));

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
