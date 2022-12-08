using UraniumCompute.Acceleration.TransientResources;
using UraniumCompute.Common.Math;

namespace UraniumCompute.Acceleration.Pipelines;

internal sealed class DeviceJobContext : IDeviceJobSetupContext, IJobRunContext
{
    public IDeviceJob Job { get; }
    public Pipeline Pipeline { get; }

    private readonly JobInitializer initializer;

    private Vector3Int workgroups;
    private Delegate? kernel;

    public DeviceJobContext(IDeviceJob job, Pipeline pipeline)
    {
        Job = job;
        Pipeline = pipeline;
        initializer = new JobInitializer(pipeline);
    }

    public IJobSetupContext CreateBuffer<T>(out TransientBuffer1D<T> buffer, Buffer1D<T>.Desc desc,
        MemoryKindFlags memoryKindFlags)
        where T : unmanaged
    {
        initializer.CreateBuffer(out buffer, desc, memoryKindFlags);
        return this;
    }

    public IJobSetupContext ReadBuffer<T>(ITransientBuffer<T> buffer)
        where T : unmanaged
    {
        return this;
    }

    public IJobSetupContext WriteBuffer<T>(ITransientBuffer<T> buffer)
        where T : unmanaged
    {
        return this;
    }

    public IDeviceJobSetupContext SetWorkgroups(int x, int y, int z)
    {
        workgroups = new Vector3Int(x, y, z);
        return this;
    }

    public void Run(Delegate jobDelegate)
    {
        kernel = jobDelegate;
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

    public void Run()
    {
    }
}
