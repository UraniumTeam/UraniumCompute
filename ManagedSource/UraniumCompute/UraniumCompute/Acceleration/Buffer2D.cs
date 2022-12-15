using UraniumCompute.Backend;
using UraniumCompute.Memory;

namespace UraniumCompute.Acceleration;

/// <inheritdoc />
public sealed class Buffer2D<T> : Buffer<T>
    where T : unmanaged
{
    /// <summary>
    ///     The width of the buffer.
    /// </summary>
    public int Width => (int)LongWidth;

    /// <summary>
    ///     The height of the buffer.
    /// </summary>
    public int Height => (int)LongHeight;

    /// <summary>
    ///     The width of the buffer as <see cref="System.Int64" />.
    /// </summary>
    public ulong LongWidth { get; private set; }

    /// <summary>
    ///     The height of the buffer as <see cref="System.Int64" />.
    /// </summary>
    public ulong LongHeight { get; private set; }

    internal Buffer2D(IntPtr handle) : base(handle)
    {
    }

    internal Buffer2D(ulong width, ulong height, IntPtr handle) : base(handle)
    {
        LongWidth = width;
        LongHeight = height;
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
    ///     Initialize the buffer.
    /// </summary>
    /// <param name="name">Debug name of the object.</param>
    /// <param name="xDimension">Size of the buffer in the x dimension.</param>
    /// <param name="yDimension">Size of the buffer in the y dimension.</param>
    public void Init(NativeString name, ulong xDimension, ulong yDimension)
    {
        LongWidth = xDimension;
        LongHeight = yDimension;
        Init(new Desc(name, xDimension, yDimension));
    }

    /// <summary>
    ///     Map the memory bound to this buffer.
    /// </summary>
    /// <returns>Memory mapping helper object that maps to buffer memory.</returns>
    public MemoryMapper2D<T> Map()
    {
        return BoundMemory.Map<T>().ReshapeUnchecked(LongWidth, LongHeight);
    }

    /// <summary>
    ///     Reshape the buffer to a <see cref="Buffer1D{T}" />.
    /// </summary>
    /// <param name="width">Width of the 1D buffer.</param>
    /// <returns>An instance of <see cref="Buffer1D{T}" /> that stores the same handle.</returns>
    public Buffer1D<T> Reshape(int width)
    {
        IncrementReferenceCount();
        return new Buffer1D<T>(Handle);
    }

    internal static BufferBase.Desc CreateDesc(Desc desc)
    {
        return new BufferBase.Desc(desc.Name, desc.XDimension * desc.YDimension * (ulong)ElementSize, Usage.Storage);
    }

    public new readonly record struct Desc(NativeString Name, ulong XDimension, ulong YDimension)
    {
        public ulong ByteSize => XDimension * (ulong)ElementSize;
    }
}
