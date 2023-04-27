using UraniumCompute.Acceleration.TransientResources;
using UraniumCompute.Backend;

namespace UraniumCompute.Acceleration.Pipelines;

public abstract class JobSetupContext : IJobSetupContext
{
    public abstract IComputeJob ComputeJob { get; }

    public List<ITransientResource> CreatedResources { get; } = new();
    public List<ITransientResource> ReadResources { get; } = new();
    public List<ITransientResource> WrittenResources { get; } = new();

    protected ulong RequiredDeviceMemoryInBytes { get; private set; }
    protected ulong RequiredHostMemoryInBytes { get; private set; }

    public Pipeline Pipeline { get; }

    protected readonly List<ITransientResource> variables = new();
    private readonly List<Action> initActions = new();

    protected JobSetupContext(Pipeline pipeline)
    {
        Pipeline = pipeline;
    }

    public IJobSetupContext CreateBuffer<T>(out TransientBuffer1D<T> buffer, Buffer1D<T>.Desc desc,
        MemoryKindFlags memoryKindFlags) where T : unmanaged
    {
        buffer = new TransientBuffer1D<T>(desc, memoryKindFlags)
        {
            Creator = ComputeJob
        };

        CreatedResources.Add(buffer);
        Pipeline.AddResource(buffer);

        var resource = buffer;
        initActions.Add(() => InitBuffer(resource));

        if (memoryKindFlags.HasFlag(MemoryKindFlags.HostAccessible))
        {
            RequiredHostMemoryInBytes += desc.ByteSize;
        }
        else
        {
            RequiredDeviceMemoryInBytes += desc.ByteSize;
        }

        variables.Add(buffer);
        return this;
    }

    private void InitBuffer<T>(TransientBuffer1D<T> resource)
        where T : unmanaged
    {
        var buffer = Pipeline.GetTransientResourceHeap(resource.MemoryKindFlags)
            .CreateBuffer1D(resource.Id, resource.Descriptor, this);
        Pipeline.InitResource(resource.Id, buffer);
    }

    public IJobSetupContext Read(ITransientResource resource)
    {
        variables.Add(resource);
        ReadResources.Add(resource);
        resource.Readers.Add(ComputeJob);
        // TODO: for now we just set the latest reader or writer of the resource as the deleter
        // Later, when job cancelling will be implemented this will no longer work
        resource.Deleter = ComputeJob;
        return this;
    }

    public IJobSetupContext Write(ITransientResource resource)
    {
        variables.Add(resource);
        WrittenResources.Add(resource);
        resource.Writers.Add(ComputeJob);
        resource.Deleter = ComputeJob;
        return this;
    }

    public abstract void Run(ICommandRecordingContext ctx);
    public abstract void Setup(out ulong requiredDeviceMemory, out ulong requiredHostMemory);
    public abstract void AddBarrier(in MemoryBarrierDesc barrier, BufferBase resource);

    public abstract void Dispose();

    public virtual void Init()
    {
        foreach (var action in initActions)
        {
            action();
        }

        foreach (var resource in variables)
        {
            if (resource.Deleter == ComputeJob)
            {
                Pipeline.GetTransientResourceHeap(resource.MemoryKindFlags)
                    .ReleaseResource(resource.Id);
            }
        }
    }
}
