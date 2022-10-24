using UraniumCompute.Backend;
using UraniumCompute.Memory;

namespace UraniumCompute.Acceleration;

/// <inheritdoc />
public sealed class Buffer2D<T> : Buffer<T>
    where T : unmanaged
{
    public ulong LongWidth { get; private set; }
    public ulong LongHeight { get; private set; }
    
    public int Width => (int)LongWidth;
    public int Height => (int)LongHeight;

    internal Buffer2D(IntPtr handle) : base(handle)
    {
    }

    internal Buffer2D(ulong width, ulong height, IntPtr handle) : base(handle)
    {
        LongWidth = width;
        LongHeight = height;
    }

    public void Init(NativeString name, ulong xDimension, ulong yDimension)
    {
        LongWidth = xDimension;
        LongHeight = yDimension;
        Init(new Desc(name, xDimension * yDimension * (ulong)ElementSize));
    }

    /// <summary>
    ///     Map the memory bound to this buffer.
    /// </summary>
    /// <returns>Memory mapping helper object that maps to buffer memory.</returns>
    public MemoryMapper2D<T> Map()
    {
        return BoundMemory.Map<T>().ReshapeUnchecked(LongWidth, LongHeight);
    }

    public Buffer1D<T> Reshape(int width)
    {
        IncrementReferenceCount();
        return new Buffer1D<T>(Handle);
    }
}
