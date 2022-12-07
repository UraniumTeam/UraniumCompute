using UraniumCompute.Acceleration.TransientResources;
using UraniumCompute.Memory;

namespace UraniumCompute.Acceleration.Pipelines;

public interface IJobInitContext : IJobContext
{
    IJobInitContext CreateBuffer<T>(out TransientBuffer1D<T> buffer, NativeString name, long xDimension, MemoryKindFlags memoryKindFlags) where T : unmanaged;
    IJobInitContext ReadBuffer<T>(ITransientBuffer<T> buffer) where T : unmanaged;
    IJobInitContext WriteBuffer<T>(ITransientBuffer<T> buffer) where T : unmanaged;
}
