namespace UraniumCompute.Acceleration.Pipelines;

public interface IHostJob : IComputeJob
{
    IJobSetupContext Setup(IHostJobSetupContext ctx);
}
