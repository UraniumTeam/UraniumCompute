namespace UraniumCompute.Acceleration.Pipelines;

internal sealed class DelegateHostJob : IHostJob
{
    public string Name { get; }

    private readonly Func<IHostJobSetupContext, IJobSetupContext> initializer;
    private readonly Action jobDelegate;

    public DelegateHostJob(string name, Func<IHostJobSetupContext, IJobSetupContext> initializer, Action jobDelegate)
    {
        Name = name;
        this.initializer = initializer;
        this.jobDelegate = jobDelegate;
    }

    public IJobSetupContext Setup(IHostJobSetupContext ctx)
    {
        return initializer(ctx);
    }

    public void Run(IJobRunContext ctx)
    {
        ctx.Run(jobDelegate);
    }
}
