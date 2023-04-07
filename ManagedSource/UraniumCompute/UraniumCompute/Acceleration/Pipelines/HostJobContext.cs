using UraniumCompute.Backend;

namespace UraniumCompute.Acceleration.Pipelines;

internal sealed class HostJobContext : JobSetupContext, IHostJobSetupContext, IJobRunContext
{
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
    }

    public override void Setup(out ulong requiredDeviceMemory, out ulong requiredHostMemory)
    {
        Job.Setup(this);
        requiredDeviceMemory = RequiredDeviceMemoryInBytes;
        requiredHostMemory = RequiredHostMemoryInBytes;
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
