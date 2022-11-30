namespace UraniumCompute.Acceleration.Pipelines;

internal sealed class DelegateHostJob : IHostJob
{
    public string Name { get; }

    private readonly Func<IHostJobInitContext, IJobInitContext> initializer;
    private readonly Action jobDelegate;

    public DelegateHostJob(string name, Func<IHostJobInitContext, IJobInitContext> initializer, Action jobDelegate)
    {
        Name = name;
        this.initializer = initializer;
        this.jobDelegate = jobDelegate;
    }

    public IJobInitContext Init(IHostJobInitContext ctx)
    {
        return initializer(ctx);
    }

    public void Run(IJobRunContext ctx)
    {
        ctx.Run(jobDelegate);
    }
}
