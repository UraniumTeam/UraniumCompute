using UraniumCompute.Acceleration.TransientResources;
using UraniumCompute.Backend;
using UraniumCompute.Memory;

namespace UraniumCompute.Acceleration.Pipelines;

internal sealed class HostJobContext : IHostJobSetupContext, IJobRunContext
{
    public IHostJob Job { get; }
    public Pipeline Pipeline { get; }

    public List<ITransientResource> CreatedResources => initializer.CreatedResources;
    public List<ITransientResource> ReadResources { get; } = new();
    public List<ITransientResource> WrittenResources { get; } = new();

    private readonly JobInitializer initializer;
    private Action? kernel;

    public HostJobContext(IHostJob job, Pipeline pipeline)
    {
        Job = job;
        Pipeline = pipeline;
        initializer = new JobInitializer(pipeline);
    }

    public IJobSetupContext CreateBuffer<T>(out TransientBuffer1D<T> buffer, Buffer1D<T>.Desc desc,
        MemoryKindFlags memoryKindFlags) where T : unmanaged
    {
        initializer.CreateBuffer(out buffer, desc, memoryKindFlags);
        return this;
    }

    public IJobSetupContext ReadBuffer<T>(ITransientBuffer<T> buffer) where T : unmanaged
    {
        ReadResources.Add(buffer);
        return this;
    }

    public IJobSetupContext WriteBuffer<T>(ITransientBuffer<T> buffer) where T : unmanaged
    {
        WrittenResources.Add(buffer);
        return this;
    }

    public void Run(Delegate jobDelegate)
    {
        kernel = (Action)jobDelegate;
    }

    public void Dispose()
    {
    }

    public void Setup(out ulong requiredDeviceMemory, out ulong requiredHostMemory)
    {
        Job.Setup(this);
        requiredDeviceMemory = initializer.RequiredDeviceMemoryInBytes;
        requiredHostMemory = initializer.RequiredHostMemoryInBytes;
    }

    public void Init()
    {
        initializer.Init();
        Job.Run(this);
    }

    public void Run(ICommandRecordingContext ctx)
    {
        kernel!();
    }
}
