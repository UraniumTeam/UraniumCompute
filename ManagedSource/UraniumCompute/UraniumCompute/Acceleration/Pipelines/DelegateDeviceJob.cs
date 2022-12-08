namespace UraniumCompute.Acceleration.Pipelines;

internal sealed class DelegateDeviceJob : IDeviceJob
{
    public string Name { get; }

    private readonly Func<IDeviceJobSetupContext, IJobSetupContext> initializer;
    private readonly Delegate kernel;

    public DelegateDeviceJob(string name, Func<IDeviceJobSetupContext, IJobSetupContext> initializer, Delegate kernel)
    {
        Name = name;
        this.initializer = initializer;
        this.kernel = kernel;
    }

    public IJobSetupContext Setup(IDeviceJobSetupContext ctx)
    {
        return initializer(ctx);
    }

    public void Run(IJobRunContext ctx)
    {
        ctx.Run(kernel);
    }
}