using UraniumCompute.Acceleration.TransientResources;

namespace UraniumCompute.Acceleration.Pipelines;

public sealed class Pipeline : IDisposable
{
    public JobScheduler JobScheduler { get; }
    public bool IsInitialized { get; private set; }
    public TransientResourceHeap TransientResourceHeap { get; }

    private readonly List<IJobContext> jobs = new();

    internal Pipeline(JobScheduler scheduler)
    {
        JobScheduler = scheduler;
        TransientResourceHeap = new TransientResourceHeap(JobScheduler.Device);
    }

    public T AddHostJob<T>(T job)
        where T : IHostJob
    {
        if (IsInitialized)
        {
            throw new InvalidOperationException("Pipeline was already initialized");
        }

        jobs.Add(new HostJobContext(job, this));
        return job;
    }

    public T AddDeviceJob<T>(T job)
        where T : IDeviceJob
    {
        if (IsInitialized)
        {
            throw new InvalidOperationException("Pipeline was already initialized");
        }

        jobs.Add(new DeviceJobContext(job, this));
        return job;
    }

    public IHostJob AddHostJob(string name, Func<IHostJobInitContext, IJobInitContext> initializer, Action jobDelegate)
    {
        return AddHostJob(new DelegateHostJob(name, initializer, jobDelegate));
    }

    public IDeviceJob AddDeviceJob(string name, Func<IDeviceJobInitContext, IJobInitContext> initializer, Delegate kernel)
    {
        return AddDeviceJob(new DelegateDeviceJob(name, initializer, kernel));
    }

    public async Task Run()
    {
        if (!IsInitialized)
        {
            foreach (var jobContext in jobs)
            {
                jobContext.Init();
            }

            IsInitialized = true;
        }

        foreach (var jobContext in jobs)
        {
            jobContext.Run();
        }

        await Task.Delay(1);
    }

    public void Dispose()
    {
        TransientResourceHeap.Dispose();
        foreach (var jobContext in jobs)
        {
            jobContext.Dispose();
        }
    }
}
