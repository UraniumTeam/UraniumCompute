using System.Collections;
using System.Runtime.CompilerServices;

namespace UraniumCompute.Backend;

public sealed class MemoryMapHelper<T> : IDisposable, IReadOnlyList<T>
    where T : unmanaged
{
    private readonly DeviceMemorySlice memorySlice;
    private readonly unsafe T* mapPointer;

    public static readonly unsafe ulong ElementSize = (ulong)sizeof(T);

    internal unsafe MemoryMapHelper(in DeviceMemorySlice slice, T* map)
    {
        memorySlice = slice;
        mapPointer = map;
    }

    internal unsafe MemoryMapHelper(DeviceMemory memory, ulong offset, ulong size, T* map)
        : this(new DeviceMemorySlice(memory, offset, size), map)
    {
    }

    public void Dispose()
    {
        memorySlice.Unmap();
    }

    public IEnumerator<T> GetEnumerator()
    {
        for (ulong i = 0; i < LongCount; i++)
        {
            yield return GetElementAtUnchecked(i);
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int Count => (int)LongCount;

    public ulong LongCount => memorySlice.Size / ElementSize;

    public T this[int index]
    {
        get => GetElementAt(index);
        set => GetElementAt(index) = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe ref T GetElementAt(long index)
    {
        if (index < 0 || (ulong)index > LongCount)
        {
            throw new IndexOutOfRangeException("Memory map out of range");
        }

        return ref mapPointer[index];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe ref T GetElementAt(ulong index)
    {
        if (index > LongCount)
        {
            throw new IndexOutOfRangeException("Memory map out of range");
        }

        return ref mapPointer[index];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe ref T GetElementAtUnchecked(ulong index)
    {
        return ref mapPointer[index];
    }
}
