using UraniumCompute.Backend;
using UraniumCompute.Memory;

namespace UraniumCompute.Acceleration;

/// <inheritdoc />
public sealed class Buffer<T> : BufferBase
    where T : unmanaged
{
    public static readonly unsafe int ElementSize = sizeof(T);

    public ulong LongCount => Descriptor.Size / (ulong)ElementSize;

    public int Count => (int)LongCount;

    internal Buffer(IntPtr handle) : base(handle)
    {
    }

    /// <summary>
    ///     Initialize buffer with element count instead of size in bytes.
    /// </summary>
    /// <param name="name">Debug name of the object.</param>
    /// <param name="elementCount">The number of elements stored in the buffer.</param>
    public void Init(NativeString name, ulong elementCount)
    {
        Init(new Desc(name, elementCount * (ulong)ElementSize));
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
