using System.Runtime.CompilerServices;

namespace UraniumCompute.Backend;

/// <summary>
///     Helper class for memory mapping, implements indexing operators and bound checking.
/// </summary>
/// <typeparam name="T">Type of the element stored in the mapped memory.</typeparam>
public abstract class MemoryMapper<T> : IDisposable
    where T : unmanaged
{
    /// <summary>
    ///     Size of a single element in bytes.
    /// </summary>
    public static readonly unsafe ulong ElementSize = (ulong)sizeof(T);

    /// <summary>
    ///     The number of elements in the mapped memory.
    /// </summary>
    public int Count => (int)LongCount;

    /// <summary>
    ///     The number of elements in the mapped memory as <see cref="System.Int64"/>.
    /// </summary>
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
