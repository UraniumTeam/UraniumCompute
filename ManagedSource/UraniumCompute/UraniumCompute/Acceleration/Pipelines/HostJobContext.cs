namespace UraniumCompute.Acceleration.Pipelines;

internal sealed class HostJobContext : IHostJobInitContext, IJobRunContext
{
    public IHostJob Job { get; }
    public Pipeline Pipeline { get; }

    public HostJobContext(IHostJob job, Pipeline pipeline)
    {
        Job = job;
        Pipeline = pipeline;
    }

    public IJobInitContext InitBuffer<T>(Buffer1D<T> buffer, long xDimension, MemoryKindFlags memoryKindFlags) where T : unmanaged
    {
        throw new NotImplementedException();
    }

    public IJobInitContext ReadBuffer<T>(Buffer<T> buffer) where T : unmanaged
    {
        throw new NotImplementedException();
    }

    public IJobInitContext WriteBuffer<T>(Buffer<T> buffer) where T : unmanaged
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
