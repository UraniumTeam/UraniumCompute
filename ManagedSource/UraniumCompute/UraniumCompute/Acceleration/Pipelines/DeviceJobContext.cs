using UraniumCompute.Backend;
using UraniumCompute.Common.Math;
using UraniumCompute.Compilation;

namespace UraniumCompute.Acceleration.Pipelines;

internal sealed class DeviceJobContext : JobSetupContext, IDeviceJobSetupContext, IJobRunContext
{
    public override IComputeJob ComputeJob => Job;
    public IDeviceJob Job { get; }

    private readonly Kernel kernel;
    private readonly ResourceBinding resourceBinding;

    private readonly List<(MemoryBarrierDesc, BufferBase)> barriers = new();

    private Vector3Int workgroupCount;
    private int workgroupSize = 1;

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

    public IDeviceJobSetupContext SetWorkgroupSize(int size)
    {
        workgroupSize = size;
        return this;
    }

    public void Run(Delegate jobDelegate)
    {
        CompilerUtils.CompileKernel(jobDelegate, Pipeline.JobScheduler.KernelCompiler, kernel, resourceBinding, workgroupSize);
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

        if (workgroupCount == Vector3Int.Zero)
        {
            throw new ArgumentException("Workgroups must be set for all device jobs");
        }
    }

    public override void AddBarrier(in MemoryBarrierDesc barrier, BufferBase resource)
    {
        barriers.Add((barrier, resource));
    }

    public override void Init()
    {
        base.Init();
        Job.Run(this);
    }

    public override void Run(ICommandRecordingContext ctx)
    {
        foreach (var barrier in barriers)
        {
            ctx.MemoryBarrierUnsafe(barrier.Item2, barrier.Item1);
        }

        ctx.Dispatch(kernel, workgroupCount);
    }

    public override void Dispose()
    {
        kernel.Dispose();
        resourceBinding.Dispose();
    }
}
