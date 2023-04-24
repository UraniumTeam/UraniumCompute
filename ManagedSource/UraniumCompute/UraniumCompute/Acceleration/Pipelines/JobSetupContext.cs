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
            .CreateBuffer1D(resource.Id, resource.Descriptor, out var info);
        Pipeline.InitResource(resource.Id, buffer);
        // TODO: place barrier
    }

    public IJobSetupContext ReadBuffer<T>(ITransientBuffer<T> buffer) where T : unmanaged
    {
        variables.Add(buffer);
        ReadResources.Add(buffer);
        buffer.Readers.Add(ComputeJob);
        // TODO: for now we just set the latest reader or writer of the resource as the deleter
        // Later, when job cancelling will be implemented this will no longer work
        buffer.Deleter = ComputeJob;
        return this;
    }

    public IJobSetupContext WriteBuffer<T>(ITransientBuffer<T> buffer) where T : unmanaged
    {
        variables.Add(buffer);
        WrittenResources.Add(buffer);
        buffer.Writers.Add(ComputeJob);
        buffer.Deleter = ComputeJob;
        return this;
    }

    public abstract void Run(ICommandRecordingContext ctx);
    public abstract void Setup(out ulong requiredDeviceMemory, out ulong requiredHostMemory);
    public abstract void AddBarrier(in MemoryBarrierDesc barrier, BufferBase resource);

    public abstract void Dispose();

    public virtual void Init()
    {
        foreach (var resource in variables)
        {
            if (resource.Deleter == ComputeJob)
            {
                Pipeline.GetTransientResourceHeap(resource.MemoryKindFlags)
                    .ReleaseResource(resource.Id);
            }
        }

        foreach (var action in initActions)
        {
            action();
        }
    }
}
