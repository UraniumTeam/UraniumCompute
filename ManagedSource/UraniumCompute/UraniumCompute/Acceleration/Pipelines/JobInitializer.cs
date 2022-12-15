using UraniumCompute.Acceleration.TransientResources;

namespace UraniumCompute.Acceleration.Pipelines;

internal sealed class JobInitializer
{
    public ulong RequiredDeviceMemoryInBytes { get; private set; }
    public ulong RequiredHostMemoryInBytes { get; private set; }
    public List<ITransientResource> CreatedResources { get; } = new();
    
    private readonly List<Action> initActions = new();
    private readonly Pipeline pipeline;

    public JobInitializer(Pipeline pipeline)
    {
        this.pipeline = pipeline;
    }

    public void CreateBuffer<T>(out TransientBuffer1D<T> buffer, Buffer1D<T>.Desc desc, MemoryKindFlags memoryKindFlags) where T : unmanaged
    {
        var id = pipeline.AddResource(desc);
        buffer = new TransientBuffer1D<T>(pipeline, id);
        initActions.Add(() => InitBuffer(id, desc, memoryKindFlags));
        if (memoryKindFlags.HasFlag(MemoryKindFlags.HostAccessible))
        {
            RequiredHostMemoryInBytes += desc.ByteSize;
        }
        else
        {
            RequiredDeviceMemoryInBytes += desc.ByteSize;
        }
    }

    private void InitBuffer<T>(int id, Buffer1D<T>.Desc desc, MemoryKindFlags memoryKindFlags)
        where T : unmanaged
    {
        var buffer = pipeline.GetTransientResourceHeap(memoryKindFlags).CreateBuffer1D(id, desc, out var info);
        pipeline.InitResource(id, buffer);
        // TODO: place barrier
    }

    public void Init()
    {
        foreach (var action in initActions)
        {
            action();
        }
    }
}
