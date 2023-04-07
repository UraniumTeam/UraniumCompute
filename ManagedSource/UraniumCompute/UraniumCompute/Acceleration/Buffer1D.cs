using UraniumCompute.Backend;
using UraniumCompute.Memory;

namespace UraniumCompute.Acceleration;

/// <inheritdoc />
public sealed class Buffer1D<T> : Buffer<T>
    where T : unmanaged
{
    internal Buffer1D(nint handle) : base(handle)
    {
    }
    
    /// <summary>
    ///     Initialize buffer with element count instead of size in bytes.
    /// </summary>
    /// <param name="desc">Device object descriptor.</param>
    public void Init(Desc desc)
    {
        Init(CreateDesc(desc));
    }

    /// <summary>
    ///     Initialize buffer with element count instead of size in bytes.
    /// </summary>
    /// <param name="name">Debug name of the object.</param>
    /// <param name="xDimension">The number of elements stored in the buffer along the x-axis.</param>
    public void Init(NativeString name, ulong xDimension)
    {
        Init(new Desc(name, xDimension));
    }

    /// <summary>
    ///     Map the memory bound to this buffer.
    /// </summary>
    /// <returns>Memory mapping helper object that maps to buffer memory.</returns>
    public MemoryMapper1D<T> Map()
    {
        return BoundMemory.Map<T>();
    }

    /// <summary>
    ///     Reshape the buffer to a <see cref="Buffer2D{T}" />.
    /// </summary>
    /// <param name="width">Width of the 2D buffer.</param>
    /// <param name="height">Height of the 2D buffer.</param>
    /// <returns>An instance of <see cref="Buffer2D{T}" /> that stores the same handle.</returns>
    /// <exception cref="ArgumentException">Invalid dimensions: width * height != this.Count</exception>
    public Buffer2D<T> Reshape(long width, long height)
    {
        if (height < 0)
        {
            height = (long)LongCount / width;
        }

        if (width * height != (long)LongCount)
        {
            throw new ArgumentException(
                $"Invalid shape: {width} * {height} = {width * height}, but element count was {LongCount}");
        }

        IncrementReferenceCount();
        return new Buffer2D<T>((ulong)width, (ulong)height, Handle);
    }

    internal static BufferBase.Desc CreateDesc(in Desc desc)
    {
        return new BufferBase.Desc(desc.Name, desc.XDimension * (ulong)ElementSize, Usage.Storage);
    }

    public new readonly record struct Desc(NativeString Name, ulong XDimension)
    {
        public ulong ByteSize => XDimension * (ulong)ElementSize;
    }
}
