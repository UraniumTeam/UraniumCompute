using UraniumCompute.Backend;

namespace UraniumCompute.Acceleration;

public class DeviceFactory : IDisposable
{
    public BackendKind BackendKind { get; }
    public IReadOnlyCollection<AdapterInfo> Adapters { get; }

    private DeviceFactory(BackendKind backendKind)
    {
        BackendKind = backendKind;
    }

    public static DeviceFactory Create(BackendKind backendKind)
    {
        return new DeviceFactory(backendKind);
    }

    public void Dispose()
    {
    }
}
