using UraniumCompute.Acceleration;
using UraniumCompute.Backend;

namespace SampleProject;

internal static class Program
{
    public static void Main()
    {
        using var factory = DeviceFactory.Create(BackendKind.Vulkan);
        var adapters = factory.Adapters;
    }
}
