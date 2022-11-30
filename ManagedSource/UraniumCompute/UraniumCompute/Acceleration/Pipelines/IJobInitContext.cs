namespace UraniumCompute.Acceleration.Pipelines;

public interface IJobInitContext
{
    IJobInitContext InitBuffer<T>(Buffer1D<T> buffer, long xDimension, MemoryKindFlags memoryKindFlags) where T : unmanaged;
    IJobInitContext ReadBuffer<T>(Buffer<T> buffer) where T : unmanaged;
    IJobInitContext WriteBuffer<T>(Buffer<T> buffer) where T : unmanaged;
}
