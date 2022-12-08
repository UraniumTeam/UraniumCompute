namespace UraniumCompute.Acceleration.Pipelines;

public interface IDeviceJobSetupContext : IJobSetupContext
{
    IDeviceJobSetupContext SetWorkgroups(int x, int y, int z);
}
