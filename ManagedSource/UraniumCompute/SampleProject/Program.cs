using System.Diagnostics;
using System.Reflection;
using UraniumCompute.Acceleration;
using UraniumCompute.Backend;
using UraniumCompute.Compilation;
using UraniumCompute.Compiler.Decompiling;
using UraniumCompute.Utils;
using UraniumCompute.Compiler.InterimStructs;

namespace SampleProject;

internal static class Program
{
    public static void Main()
    {
        using var factory = DeviceFactory.Create(BackendKind.Vulkan);
        factory.Init(new DeviceFactory.Desc(Assembly.GetExecutingAssembly().FullName!));

        foreach (ref readonly var adapter in factory.Adapters)
        {
            Console.WriteLine(adapter);
        }

        using var device = factory.CreateDevice();
        device.Init(new ComputeDevice.Desc(factory.Adapters.FirstDiscrete().Id));

        using var hostBuffer = device.CreateBuffer1D<float>();
        hostBuffer.Init("Test Buffer", 1024);

        using var hostMemory = hostBuffer.AllocateMemory("Host Memory", MemoryKindFlags.HostAndDeviceAccessible);
        hostBuffer.BindMemory(new DeviceMemorySlice(hostMemory));

        // Unsafe memory access through the device memory interface directly:
        using (var map = hostMemory.Map<int>())
        {
            for (var i = 0; i < map.Count; i++)
            {
                map[i] = 1;
            }
        }

        using (var map = hostMemory.Map<long>())
        {
            Trace.Assert(map[0] == (long)uint.MaxValue + 2);
            Console.WriteLine($"Buffer memory: {string.Join(", ", map.Take(8))}...");
        }

        // Type safe memory access through the buffer interface:
        using (var map = hostBuffer.Map())
        {
            for (var i = 0; i < map.Count; i++)
            {
                map[i] = i + 0.5f;
            }
        }

        using (var map = hostBuffer.Map())
        {
            Trace.Assert(Math.Abs(map[5] - 5.5f) < 0.001f);
            Console.WriteLine($"Buffer memory: {string.Join(", ", map.Take(8))}...");
        }

        Console.WriteLine($"{device}, {hostBuffer}, {hostMemory}");

        using var fence = device.CreateFence();
        fence.Init(new Fence.Desc("Test fence", FenceState.Reset));

        fence.SignalOnCpu();
        fence.WaitOnCpu(TimeSpan.FromMilliseconds(1)).ThrowOnError();

        using var commandList = device.CreateCommandList();
        commandList.Init(
            new CommandList.Desc("Test command list", HardwareQueueKindFlags.Compute, CommandListFlags.OneTimeSubmit));

        using var deviceBuffer = device.CreateBuffer1D<float>();
        deviceBuffer.Init(hostBuffer.Descriptor with { Name = "Copied from test buffer" });

        using var deviceMemory = hostBuffer.AllocateMemory("Device memory", MemoryKindFlags.DeviceAccessible);
        deviceBuffer.BindMemory(new DeviceMemorySlice(deviceMemory));

        using (var cmd = commandList.Begin())
        {
            Console.WriteLine(commandList.State);
            cmd.Copy(hostBuffer, deviceBuffer);
        }

        Console.WriteLine(commandList.State);
        commandList.Submit();
        Console.WriteLine(commandList.State);
        commandList.CompletionFence.WaitOnCpu();
        Console.WriteLine(commandList.State);

        commandList.ResetState();
        using (var cmd = commandList.Begin())
        {
            cmd.Copy(deviceBuffer, hostBuffer);
        }

        commandList.Submit();
        commandList.CompletionFence.WaitOnCpu();

        using (var map = hostBuffer.Map())
        {
            Trace.Assert(Math.Abs(map[5] - 5.5f) < 0.001f);
            Console.WriteLine($"Buffer copied to GPU, than back to CPU accessible memory: {string.Join(", ", map.Take(8))}...");
        }

        using var kernelCompiler = factory.CreateKernelCompiler();
        kernelCompiler.Init(new KernelCompiler.Desc("Kernel compiler"));
        using var resourceBinding = device.CreateResourceBinding();
        using var kernel = device.CreateKernel();

        CompilerUtils.CompileKernel((Span<float> values) => { values[GpuIntrinsic.GetGlobalInvocationId().X] *= 2; },
            kernelCompiler, kernel, resourceBinding);

        resourceBinding.SetVariable(0, deviceBuffer);

        commandList.ResetState();
        using (var cmd = commandList.Begin())
        {
            cmd.Dispatch(kernel, deviceBuffer.Count, 1, 1);
            cmd.MemoryBarrier(deviceBuffer, AccessFlags.KernelWrite, AccessFlags.TransferRead);
            cmd.Copy(deviceBuffer, hostBuffer);
        }

        commandList.Submit();
        commandList.CompletionFence.WaitOnCpu();

        using (var map = hostBuffer.Map())
        {
            Trace.Assert(Math.Abs(map[5] - 2 * 5.5f) < 0.001f);
            Console.WriteLine($"Kernel calculation results: {string.Join(", ", map.Take(8))}...");
        }
    }
}
