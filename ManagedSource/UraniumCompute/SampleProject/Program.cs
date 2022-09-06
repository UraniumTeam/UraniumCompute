using System.Diagnostics;
using System.Reflection;
using UraniumCompute.Acceleration;
using UraniumCompute.Backend;

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
        device.Init(new ComputeDevice.Desc(factory.Adapters[0].Id));

        using var buffer = device.CreateBuffer<float>();
        buffer.Init(new BufferBase.Desc("Buffer", 1024));

        using var memory = buffer.AllocateMemory("Memory", MemoryKindFlags.HostAndDeviceAccessible);
        buffer.BindMemory(new DeviceMemorySlice(memory));

        // Type safe memory access through the buffer interface:
        using (var map = buffer.Map())
        {
            for (var i = 0; i < map.Count; i++)
            {
                map[i] = i + 0.5f;
            }
        }

        using (var map = buffer.Map())
        {
            Trace.Assert(Math.Abs(map[5] - 5.5f) < 0.001f);
            Console.WriteLine($"Buffer memory: {string.Join(", ", map)}");
        }

        // Unsafe memory access through the device memory interface directly:
        using (var map = memory.Map<int>())
        {
            for (var i = 0; i < map.Count; i++)
            {
                map[i] = 1;
            }
        }

        using (var map = memory.Map<long>())
        {
            Trace.Assert(map[0] == (long)uint.MaxValue + 2);
            Console.WriteLine($"Buffer memory: {string.Join(", ", map)}");
        }

        Console.WriteLine(buffer.DebugName);

        using var fence = device.CreateFence();
        fence.Init(new Fence.Desc("Test fence", FenceState.Reset));

        fence.SignalOnCpu();
        fence.WaitOnCpu(TimeSpan.FromMilliseconds(1)).ThrowOnError();
    }
}
