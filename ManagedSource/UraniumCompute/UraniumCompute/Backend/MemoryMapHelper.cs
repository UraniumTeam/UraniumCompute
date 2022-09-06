using System.Collections;
using System.Runtime.CompilerServices;

namespace UraniumCompute.Backend;

public sealed class MemoryMapHelper<T> : IDisposable, IReadOnlyList<T>
    where T : unmanaged
{
    public static readonly unsafe ulong ElementSize = (ulong)sizeof(T);

    public int Count => (int)LongCount;

    public ulong LongCount => memorySlice.Size / ElementSize;

    public T this[int index]
    {
        get => GetElementAt(index);
        set => GetElementAt(index) = value;
    }

    private readonly DeviceMemorySlice memorySlice;
    private readonly unsafe T* mapPointer;

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
