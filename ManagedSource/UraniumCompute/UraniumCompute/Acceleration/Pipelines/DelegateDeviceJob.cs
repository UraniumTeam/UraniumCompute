namespace UraniumCompute.Acceleration.Pipelines;

internal sealed class DelegateDeviceJob : IDeviceJob
{
    public string Name { get; }

    private readonly Func<IDeviceJobInitContext, IJobInitContext> initializer;
    private readonly Delegate kernel;

    public DelegateDeviceJob(string name, Func<IDeviceJobInitContext, IJobInitContext> initializer, Delegate kernel)
    {
        Name = name;
        this.initializer = initializer;
        this.kernel = kernel;
    }

    public IJobInitContext Init(IDeviceJobInitContext ctx)
    {
        return initializer(ctx);
    }

    public void Run(IJobRunContext ctx)
    {
        ctx.Run(kernel);
    }
}