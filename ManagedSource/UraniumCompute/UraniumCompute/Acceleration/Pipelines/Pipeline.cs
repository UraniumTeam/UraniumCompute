using UraniumCompute.Acceleration.TransientResources;
using UraniumCompute.Backend;

namespace UraniumCompute.Acceleration.Pipelines;

public sealed class Pipeline : IDisposable
{
    public JobScheduler JobScheduler { get; }
    public bool IsInitialized { get; private set; }
    public IReadOnlyCollection<ITransientResource> TransientResources => transientResources;

    private readonly List<ITransientResource> transientResources = new();
    private readonly List<BufferBase?> resources = new();
    private readonly List<object> resourceDescriptors = new();
    private readonly List<IJobContext> jobs = new();
    private readonly TransientResourceHeap deviceHeap;
    private readonly TransientResourceHeap hostHeap;
    private ulong requiredHostMemory;
    private ulong requiredDeviceMemory;

    internal Pipeline(JobScheduler scheduler)
    {
        JobScheduler = scheduler;
        deviceHeap = new TransientResourceHeap(JobScheduler.Device);
        hostHeap = new TransientResourceHeap(JobScheduler.Device);
    }

    public T AddHostJob<T>(T job)
        where T : IHostJob
    {
        if (IsInitialized)
        {
            throw new InvalidOperationException("Pipeline was already initialized");
        }

        jobs.Add(new HostJobContext(job, this));
        jobs.Last().Setup(out var d, out var h);
        requiredHostMemory += h;
        requiredDeviceMemory += d;
        return job;
    }

    public T AddDeviceJob<T>(T job)
        where T : IDeviceJob
    {
        if (IsInitialized)
        {
            throw new InvalidOperationException("Pipeline was already initialized");
        }

        jobs.Add(new DeviceJobContext(job, this));
        jobs.Last().Setup(out var d, out var h);
        requiredHostMemory += h;
        requiredDeviceMemory += d;
        return job;
    }

    public IHostJob AddHostJob(string name, Func<IHostJobSetupContext, IJobSetupContext> initializer, Action jobDelegate)
    {
        return AddHostJob(new DelegateHostJob(name, initializer, jobDelegate));
    }

    public IDeviceJob AddDeviceJob(string name, Func<IDeviceJobSetupContext, IJobSetupContext> initializer, Delegate kernel)
    {
        return AddDeviceJob(new DelegateDeviceJob(name, initializer, kernel));
    }

    public void Run()
    {
        if (!IsInitialized)
        {
            if (requiredHostMemory > 0)
            {
                hostHeap.Init(MemoryKindFlags.HostAndDeviceAccessible, requiredHostMemory);
            }

            if (requiredDeviceMemory > 0)
            {
                deviceHeap.Init(MemoryKindFlags.DeviceAccessible, requiredDeviceMemory);
            }

            foreach (var jobContext in jobs)
            {
                jobContext.Init();
            }

            IsInitialized = true;
        }

        foreach (var jobContext in jobs)
        {
            jobContext.Run();
        }
    }

    public void Dispose()
    {
        deviceHeap.Dispose();
        hostHeap.Dispose();
        foreach (var jobContext in jobs)
        {
            jobContext.Dispose();
        }
    }

    internal void AddResource(ITransientResource resource, object descriptor)
    {
        transientResources.Add(resource);
        resourceDescriptors.Add(descriptor);
        resources.Add(null);
    }

    internal BufferBase InitResource(int id, BufferBase resource)
    {
        resources[id] = resource;
        return resource;
    }

    internal BufferBase GetResource(int id)
    {
        return resources[id] ?? throw new ArgumentException($"Resource {id} was uninitialized");
    }

    internal T GetResourceDescriptor<T>(int id)
        where T : struct
    {
        return (T)resourceDescriptors[id];
    }

    public TransientResourceHeap GetTransientResourceHeap(MemoryKindFlags memoryKindFlags)
    {
        return memoryKindFlags.HasFlag(MemoryKindFlags.HostAccessible) ? hostHeap : deviceHeap;
    }
}
