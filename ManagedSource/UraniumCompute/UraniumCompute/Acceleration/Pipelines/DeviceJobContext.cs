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

    private static readonly AccessFlags[] writeFlags =
    {
        AccessFlags.KernelWrite,
        AccessFlags.TransferWrite,
        AccessFlags.HostWrite
    };

    private static readonly AccessFlags[] allFlags = Enum.GetValues<AccessFlags>()
        .Where(x => x != AccessFlags.None && x != AccessFlags.All)
        .ToArray();

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

        AddBarriers();
    }

    public override void Setup(out ulong requiredDeviceMemory, out ulong requiredHostMemory)
    {
        Job.Setup(this);
        requiredDeviceMemory = RequiredDeviceMemoryInBytes;
        requiredHostMemory = RequiredHostMemoryInBytes;
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

        if (!kernel.IsInitialized)
        {
            var source = ReadResources.Single();
            var dest = WrittenResources.SingleOrDefault() ?? CreatedResources.Single();
            var region = new BufferCopyRegion(source.Resource.Descriptor.Size);

            ctx.CopyUnsafe(source.Resource, dest.Resource, region);
            return;
        }

        if (workgroupCount == Vector3Int.Zero)
        {
            throw new ArgumentException($"Workgroups were not set for device job {ComputeJob.Name}");
        }

        ctx.Dispatch(kernel, workgroupCount);
    }

    public override void Dispose()
    {
        kernel.Dispose();
        resourceBinding.Dispose();
    }

    private void AddBarriers()
    {
        var isTransfer = !kernel.IsInitialized;
        foreach (var resource in CreatedResources)
        {
            var destAccess = isTransfer ? AccessFlags.TransferWrite : AccessFlags.KernelWrite;
            var barrier = new MemoryBarrierDesc(AccessFlags.None, destAccess);
            AddBarrier(in barrier, resource.Resource);
            resource.CurrentAccess = destAccess;
        }

        foreach (var resource in ReadResources)
        {
            var sourceAccess = AccessFlags.None;
            var destAccess = isTransfer ? AccessFlags.TransferRead : AccessFlags.KernelRead;
            foreach (var flag in writeFlags)
            {
                if (resource.CurrentAccess.HasFlag(flag))
                {
                    sourceAccess |= flag;
                }
            }

            if (sourceAccess != AccessFlags.None)
            {
                var barrier = new MemoryBarrierDesc(sourceAccess, destAccess);
                AddBarrier(in barrier, resource.Resource);
            }

            resource.CurrentAccess = destAccess;
        }

        foreach (var resource in WrittenResources)
        {
            var sourceAccess = AccessFlags.None;
            var destAccess = isTransfer
                ? AccessFlags.TransferReadWrite
                : AccessFlags.KernelReadWrite;

            foreach (var flag in allFlags)
            {
                if (resource.CurrentAccess.HasFlag(flag))
                {
                    sourceAccess |= flag;
                }
            }

            if (sourceAccess != AccessFlags.None)
            {
                var barrier = new MemoryBarrierDesc(sourceAccess, destAccess);
                AddBarrier(in barrier, resource.Resource);
            }

            resource.CurrentAccess = destAccess;
        }
    }
}
