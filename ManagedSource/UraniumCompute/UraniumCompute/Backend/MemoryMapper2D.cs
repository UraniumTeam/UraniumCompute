namespace UraniumCompute.Backend;

/// <inheritdoc cref="MemoryMapper{T}" />
public sealed class MemoryMapper2D<T> : MemoryMapper<T>
    where T : unmanaged
{
    /// <summary>
    ///     Width of the mapped memory as <see cref="System.Int64" />.
    /// </summary>
    public ulong LongWidth { get; }

    /// <summary>
    ///     Height of the mapped memory as <see cref="System.Int64" />.
    /// </summary>
    public ulong LongHeight { get; }

    /// <summary>
    ///     Width of the mapped memory.
    /// </summary>
    public int Width => (int)LongWidth;

    /// <summary>
    ///     Height of the mapped memory.
    /// </summary>
    public int Height => (int)LongHeight;

    public T this[int x, int y]
    {
        get => GetElementAt(x, y);
        set => GetElementAt(x, y) = value;
    }

    internal unsafe MemoryMapper2D(ulong width, ulong height, in DeviceMemorySlice slice, T* map) : base(in slice, map)
    {
        LongWidth = width;
        LongHeight = height;
    }

    private ref T GetElementAt(int x, int y)
    {
        return ref GetElementAt(x + y * Width);
    }
}
