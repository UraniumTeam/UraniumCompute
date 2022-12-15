namespace UraniumCompute.Acceleration.Pipelines;

public interface IDeviceJob : IComputeJob
{
    IJobSetupContext Setup(IDeviceJobSetupContext ctx);
}
