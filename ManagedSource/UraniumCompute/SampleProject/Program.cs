using UraniumCompute.Acceleration;
using UraniumCompute.Backend;

namespace SampleProject;

internal static class Program
{
    public static void Main()
    {
        using var factory = DeviceFactory.Create(BackendKind.Vulkan);
        factory.Init(new DeviceFactory.Desc("Test app")).ThrowOnError();

        foreach (ref readonly var adapter in factory.Adapters)
        {
            Console.WriteLine(adapter);
        }

        using var device = factory.CreateDevice();
        device.Init(new ComputeDevice.Desc(factory.Adapters[0].Id));
    }
}
