namespace UraniumCompute.Acceleration.Pipelines;

public interface IDeviceJobInitContext : IJobInitContext
{
    IDeviceJobInitContext SetWorkgroups(int x, int y, int z);
}
