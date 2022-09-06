using UraniumCompute.Backend;

namespace UraniumCompute.Acceleration;

/// <inheritdoc />
public sealed class Buffer<T> : BufferBase
    where T : unmanaged
{
    internal Buffer(IntPtr handle) : base(handle)
    {
    }

    /// <summary>
    ///     Map the memory bound to this buffer.
    /// </summary>
    /// <returns>Memory mapping helper object that maps to buffer memory.</returns>
    public MemoryMapHelper<T> Map()
    {
        return BoundMemory.Map<T>();
    }
}
