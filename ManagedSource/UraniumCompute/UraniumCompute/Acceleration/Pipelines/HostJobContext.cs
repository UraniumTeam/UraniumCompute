using UraniumCompute.Backend;

namespace UraniumCompute.Acceleration.Pipelines;

internal sealed class HostJobContext : JobSetupContext, IHostJobSetupContext, IJobRunContext
{
    public override IComputeJob ComputeJob => Job;
    public IHostJob Job { get; }

    private Action? kernel;

    public HostJobContext(IHostJob job, Pipeline pipeline)
        : base(pipeline)
    {
        Job = job;
    }

    public void Run(Delegate jobDelegate)
    {
        kernel = (Action)jobDelegate;

        foreach (var resource in CreatedResources)
        {
            resource.CurrentAccess = AccessFlags.HostReadWrite;
        }

        foreach (var resource in ReadResources)
        {
            resource.CurrentAccess = AccessFlags.HostRead;
        }

        foreach (var resource in WrittenResources)
        {
            resource.CurrentAccess = AccessFlags.HostReadWrite;
        }
    }

    public override void Setup(out ulong requiredDeviceMemory, out ulong requiredHostMemory)
    {
        Job.Setup(this);
        requiredDeviceMemory = RequiredDeviceMemoryInBytes;
        requiredHostMemory = RequiredHostMemoryInBytes;
    }

    public override void AddBarrier(in MemoryBarrierDesc barrier, BufferBase resource)
    {
    }

    public override void Init()
    {
        base.Init();
        Job.Run(this);
    }

    public override void Run(ICommandRecordingContext ctx)
    {
        kernel!();
    }

    public override void Dispose()
    {
    }
}
