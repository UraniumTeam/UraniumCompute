using UraniumCompute.Acceleration.TransientResources;
using UraniumCompute.Backend;
using UraniumCompute.Common.Math;
using UraniumCompute.Compilation;
using UraniumCompute.Compiler.InterimStructs;

namespace UraniumCompute.Acceleration.Pipelines;

internal sealed class DeviceJobContext : IDeviceJobSetupContext, IJobRunContext
{
    public IDeviceJob Job { get; }
    public Pipeline Pipeline { get; }

    private readonly JobInitializer initializer;
    private readonly List<ITransientResource> variables = new();

    private readonly Kernel kernel;
    private readonly ResourceBinding resourceBinding;

    private Vector3Int workgroupCount;

    public DeviceJobContext(IDeviceJob job, Pipeline pipeline)
    {
        Job = job;
        Pipeline = pipeline;
        initializer = new JobInitializer(pipeline);
        var device = Pipeline.JobScheduler.Device;
        kernel = device.CreateKernel();
        resourceBinding = device.CreateResourceBinding();
    }

    public IJobSetupContext CreateBuffer<T>(out TransientBuffer1D<T> buffer, Buffer1D<T>.Desc desc,
        MemoryKindFlags memoryKindFlags)
        where T : unmanaged
    {
        initializer.CreateBuffer(out buffer, desc, memoryKindFlags);
        variables.Add(buffer);
        return this;
    }

    public IJobSetupContext ReadBuffer<T>(ITransientBuffer<T> buffer)
        where T : unmanaged
    {
        variables.Add(buffer);
        return this;
    }

    public IJobSetupContext WriteBuffer<T>(ITransientBuffer<T> buffer)
        where T : unmanaged
    {
        variables.Add(buffer);
        return this;
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
        ctx.Dispatch(kernel, workgroupCount);
    }

    public void Dispose()
    {
        kernel.Dispose();
        resourceBinding.Dispose();
    }
}
