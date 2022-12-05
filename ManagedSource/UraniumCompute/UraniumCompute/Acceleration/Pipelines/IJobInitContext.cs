using UraniumCompute.Memory;

namespace UraniumCompute.Acceleration.Pipelines;

public interface IJobInitContext : IJobContext
{
    IJobInitContext CreateBuffer<T>(out Buffer1D<T> buffer, NativeString name, long xDimension, MemoryKindFlags memoryKindFlags) where T : unmanaged;
    IJobInitContext ReadBuffer<T>(Buffer<T> buffer) where T : unmanaged;
    IJobInitContext WriteBuffer<T>(Buffer<T> buffer) where T : unmanaged;
}
