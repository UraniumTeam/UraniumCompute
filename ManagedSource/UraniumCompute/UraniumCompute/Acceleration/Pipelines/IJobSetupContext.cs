using UraniumCompute.Acceleration.TransientResources;

namespace UraniumCompute.Acceleration.Pipelines;

public interface IJobSetupContext : IJobContext
{
    IJobSetupContext CreateBuffer<T>(out TransientBuffer1D<T> buffer, Buffer1D<T>.Desc desc, MemoryKindFlags memoryKindFlags)
        where T : unmanaged;

    IJobSetupContext ReadBuffer<T>(ITransientBuffer<T> buffer)
        where T : unmanaged;

    IJobSetupContext WriteBuffer<T>(ITransientBuffer<T> buffer)
        where T : unmanaged;
}
