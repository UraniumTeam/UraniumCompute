namespace UraniumCompute.Acceleration.Pipelines;

public sealed class Pipeline : IDisposable
{
    public JobScheduler JobScheduler { get; }

    private readonly List<IComputeJob> jobs = new();

    internal Pipeline(JobScheduler scheduler)
    {
        JobScheduler = scheduler;
    }

    public T AddHostJob<T>(T job)
        where T : IHostJob
    {
        jobs.Add(job);
        return job;
    }

    public T AddDeviceJob<T>(T job)
        where T : IDeviceJob
    {
        jobs.Add(job);
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
        await Task.Delay(1000);
    }

    public void Dispose()
    {
    }
}
