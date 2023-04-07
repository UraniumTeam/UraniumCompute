using UraniumCompute.Acceleration;
using UraniumCompute.Backend;
using UraniumCompute.Tensors;
using UraniumCompute.Utils;

using var factory = DeviceFactory.Create(BackendKind.Vulkan);
factory.Init(new DeviceFactory.Desc("Tensor sample"));

using var device = factory.CreateDevice();
device.Init(new ComputeDevice.Desc((factory.Adapters.FirstDiscreteOrNull() ?? factory.Adapters[0]).Id));

using var tensor = Tensor<float>.CreateOnHost(Tensor.Shape.Create(10, 100, 100));
for (var i = 0; i < tensor.HostStorage.Length; ++i)
{
    tensor.HostStorage[i] = 123;
}

await tensor.ToDevice(device);
Console.WriteLine(tensor.Shape);
await tensor.ToHost();

tensor.WriteTo(Console.Out);
