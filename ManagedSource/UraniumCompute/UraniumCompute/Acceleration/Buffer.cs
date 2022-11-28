using UraniumCompute.Backend;

namespace UraniumCompute.Acceleration;

/// <inheritdoc />
public abstract class Buffer<T> : BufferBase
    where T : unmanaged
{
    /// <summary>
    ///     Size of a single buffer element in bytes.
    /// </summary>
    public static readonly unsafe int ElementSize = sizeof(T);

    /// <summary>
    ///     The number of elements in the buffer as <see cref="System.Int64" />.
    /// </summary>
    public ulong LongCount => Descriptor.Size / (ulong)ElementSize;

    /// <summary>
    ///     The number of elements in the buffer.
    /// </summary>
    public int Count => (int)LongCount;

    protected Buffer(IntPtr handle) : base(handle)
    {
    }
}
