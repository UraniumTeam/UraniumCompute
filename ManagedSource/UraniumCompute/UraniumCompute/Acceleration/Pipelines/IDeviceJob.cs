namespace UraniumCompute.Acceleration.Pipelines;

public interface IDeviceJob : IComputeJob
{
    IJobInitContext Init(IDeviceJobInitContext ctx);
}
