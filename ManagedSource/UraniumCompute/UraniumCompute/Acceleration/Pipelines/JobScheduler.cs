using UraniumCompute.Backend;

namespace UraniumCompute.Acceleration.Pipelines;

public sealed class JobScheduler : IDisposable
{
    internal ComputeDevice Device { get; }

    private readonly DeviceFactory deviceFactory;

    private JobScheduler(BackendKind backendKind, Func<AdapterInfo, bool> adapterPredicate)
    {
        deviceFactory = DeviceFactory.Create(backendKind);
        Device = deviceFactory.CreateDevice();
        var adapter = SelectAdapter(adapterPredicate, deviceFactory.Adapters);
        Device.Init(new ComputeDevice.Desc(adapter.Id));
    }

    public Pipeline CreatePipeline()
    {
        return new Pipeline(this);
    }

    public static JobScheduler CreateForVulkan(AdapterKind preferredAdapterKind = AdapterKind.Discrete)
    {
        return new JobScheduler(BackendKind.Vulkan, x => x.Kind == preferredAdapterKind);
    }

    public static JobScheduler CreateForVulkan(Func<AdapterInfo, bool> adapterPredicate)
    {
        return new JobScheduler(BackendKind.Vulkan, adapterPredicate);
    }

    private static ref readonly AdapterInfo SelectAdapter(Func<AdapterInfo, bool> predicate, ReadOnlySpan<AdapterInfo> adapters)
    {
        foreach (ref readonly var adapter in adapters)
        {
            if (predicate(adapter))
            {
                return ref adapter;
            }
        }

        return ref adapters[0];
    }

    public void Dispose()
    {
        deviceFactory.Dispose();
        Device.Dispose();
    }
}
