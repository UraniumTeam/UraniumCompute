namespace UraniumCompute.Acceleration.Pipelines;

public sealed class Pipeline : IDisposable
{
    public JobScheduler JobScheduler { get; }
    public bool IsInitialized { get; private set; }

    private readonly List<IJobContext> jobs = new();

    internal Pipeline(JobScheduler scheduler)
    {
        JobScheduler = scheduler;
    }

    public T AddHostJob<T>(T job)
        where T : IHostJob
    {
        jobs.Add(new HostJobContext(job, this));
        return job;
    }

    public T AddDeviceJob<T>(T job)
        where T : IDeviceJob
    {
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
        foreach (var jobContext in jobs)
        {
            jobContext.Dispose();
        }
    }
}
