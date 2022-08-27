using System.Reflection;
using UraniumCompute.Acceleration;
using UraniumCompute.Backend;
using Buffer = UraniumCompute.Backend.Buffer;

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

        using var buffer = device.CreateBuffer();
        buffer.Init(new Buffer.Desc("Buffer", 1024));

        using var memory = buffer.AllocateMemory("Memory", MemoryKindFlags.HostAndDeviceAccessible);
        buffer.BindMemory(new DeviceMemorySlice(memory));

        Console.WriteLine(buffer.DebugName);
    }
}
