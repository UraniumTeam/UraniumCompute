using UraniumCompute.Acceleration.TransientResources;
using UraniumCompute.Memory;

namespace UraniumCompute.Acceleration.Pipelines;

internal sealed class DeviceJobContext : IDeviceJobInitContext, IJobRunContext
{
    public IDeviceJob Job { get; }
    public Pipeline Pipeline { get; }

    public DeviceJobContext(IDeviceJob job, Pipeline pipeline)
    {
        Job = job;
        Pipeline = pipeline;
    }

    public IJobInitContext CreateBuffer<T>(out TransientBuffer1D<T> buffer, NativeString name, long xDimension, MemoryKindFlags memoryKindFlags)
        where T : unmanaged
    {
        throw new NotImplementedException();
    }

    public IJobInitContext ReadBuffer<T>(ITransientBuffer<T> buffer)
        where T : unmanaged
    {
        throw new NotImplementedException();
    }

    public IJobInitContext WriteBuffer<T>(ITransientBuffer<T> buffer)
        where T : unmanaged
    {
        throw new NotImplementedException();
    }

    public IDeviceJobInitContext SetWorkgroups(int x, int y, int z)
    {
        throw new NotImplementedException();
    }

    public void Run(Delegate jobDelegate)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
    }

    public void Init()
    {
        Job.Init(this);
    }

    public void Run()
    {
        Job.Run(this);
    }
}
