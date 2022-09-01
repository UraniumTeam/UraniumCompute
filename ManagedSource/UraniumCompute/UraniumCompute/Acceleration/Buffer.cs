using UraniumCompute.Backend;
using UraniumCompute.Utils;

namespace UraniumCompute.Acceleration;

public sealed class Buffer<T> : BufferBase
    where T : unmanaged
{
    internal Buffer(IntPtr handle) : base(handle)
    {
    }

    public MemoryMapHelper<T> Map()
    {
        return BoundMemory.Map<T>();
    }
}
