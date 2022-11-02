using System.Runtime.CompilerServices;

namespace UraniumCompute.Backend;

public abstract class MemoryMapper<T> : IDisposable
    where T : unmanaged
{
    public static readonly unsafe ulong ElementSize = (ulong)sizeof(T);

    public int Count => (int)LongCount;

    public ulong LongCount => memorySlice.Size / ElementSize;

    protected readonly DeviceMemorySlice memorySlice;
    protected readonly unsafe T* mapPointer;

    internal unsafe MemoryMapper(in DeviceMemorySlice slice, T* map)
    {
        memorySlice = slice;
        mapPointer = map;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        memorySlice.Unmap();
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected unsafe ref T GetElementAt(long index)
    {
        if (index < 0 || (ulong)index > LongCount)
        {
            throw new IndexOutOfRangeException("Memory map out of range");
        }

        return ref mapPointer[index];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected unsafe ref T GetElementAt(ulong index)
    {
        if (index > LongCount)
        {
            throw new IndexOutOfRangeException("Memory map out of range");
        }

        return ref mapPointer[index];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected unsafe ref T GetElementAtUnchecked(ulong index)
    {
        return ref mapPointer[index];
    }
}
