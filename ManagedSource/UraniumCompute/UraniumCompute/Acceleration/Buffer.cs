using UraniumCompute.Backend;

namespace UraniumCompute.Acceleration;

/// <inheritdoc />
public abstract class Buffer<T> : BufferBase
    where T : unmanaged
{
    public static readonly unsafe int ElementSize = sizeof(T);

    public ulong LongCount => Descriptor.Size / (ulong)ElementSize;

    public int Count => (int)LongCount;

    protected Buffer(IntPtr handle) : base(handle)
    {
    }
}
