using UraniumCompute.Acceleration.TransientResources;
using UraniumCompute.Backend;

namespace UraniumCompute.Acceleration.Pipelines;

public abstract class JobSetupContext : IJobSetupContext
{
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
        var id = Pipeline.AddResource(desc);
        buffer = new TransientBuffer1D<T>(Pipeline, id);
        initActions.Add(() => InitBuffer(id, desc, memoryKindFlags));
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

    private void InitBuffer<T>(int id, Buffer1D<T>.Desc desc, MemoryKindFlags memoryKindFlags)
        where T : unmanaged
    {
        var buffer = Pipeline.GetTransientResourceHeap(memoryKindFlags).CreateBuffer1D(id, desc, out var info);
        Pipeline.InitResource(id, buffer);
        // TODO: place barrier
    }

    public IJobSetupContext ReadBuffer<T>(ITransientBuffer<T> buffer) where T : unmanaged
    {
        variables.Add(buffer);
        ReadResources.Add(buffer);
        return this;
    }

    public IJobSetupContext WriteBuffer<T>(ITransientBuffer<T> buffer) where T : unmanaged
    {
        variables.Add(buffer);
        WrittenResources.Add(buffer);
        return this;
    }

    public abstract void Run(ICommandRecordingContext ctx);
    public abstract void Setup(out ulong requiredDeviceMemory, out ulong requiredHostMemory);

    public abstract void Dispose();

    public virtual void Init()
    {
        foreach (var action in initActions)
        {
            action();
        }
    }
}
