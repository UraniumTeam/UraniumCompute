namespace UraniumCompute.Backend;

public sealed class MemoryMapper2D<T> : MemoryMapper<T>
    where T : unmanaged
{
    public ulong LongWidth { get; }
    public ulong LongHeight { get; }
    
    public int Width => (int)LongWidth;
    public int Height => (int)LongHeight;

    public T this[int x, int y]
    {
        get => GetElementAt(x, y);
        set => GetElementAt(x, y) = value;
    }

    private ref T GetElementAt(int x, int y)
    {
        return ref GetElementAt(x + y * Width);
    }

    internal unsafe MemoryMapper2D(ulong width, ulong height, in DeviceMemorySlice slice, T* map) : base(in slice, map)
    {
        LongWidth = width;
        LongHeight = height;
    }
}
