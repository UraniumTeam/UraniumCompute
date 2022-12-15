using UraniumCompute.Common.Math;

namespace UraniumCompute.Acceleration.Pipelines;

public interface IDeviceJobSetupContext : IJobSetupContext
{
    IDeviceJobSetupContext SetWorkgroups(Vector3Int workgroups);
}
