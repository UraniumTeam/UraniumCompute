using UraniumCompute.Backend;
using UraniumCompute.Common.Math;
using UraniumCompute.Compilation;

namespace UraniumCompute.Acceleration.Pipelines;

internal sealed class DeviceJobContext : JobSetupContext, IDeviceJobSetupContext, IJobRunContext
{
    public IDeviceJob Job { get; }

    private readonly Kernel kernel;
    private readonly ResourceBinding resourceBinding;

    private Vector3Int workgroupCount;

    public DeviceJobContext(IDeviceJob job, Pipeline pipeline)
        : base(pipeline)
    {
        Job = job;
        var device = Pipeline.JobScheduler.Device;
        kernel = device.CreateKernel();
        resourceBinding = device.CreateResourceBinding();
    }

    public IDeviceJobSetupContext SetWorkgroups(Vector3Int workgroups)
    {
        workgroupCount = workgroups;
        return this;
    }

    public void Run(Delegate jobDelegate)
    {
        CompilerUtils.CompileKernel(jobDelegate, Pipeline.JobScheduler.KernelCompiler, kernel, resourceBinding);
        for (var i = 0; i < variables.Count; ++i)
        {
            resourceBinding.SetVariableInternal(i, variables[i].Resource);
        }
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
        ctx.Dispatch(kernel, workgroupCount);
    }

    public override void Dispose()
    {
        kernel.Dispose();
        resourceBinding.Dispose();
    }
}
