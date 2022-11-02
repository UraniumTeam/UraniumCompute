using System.Collections;
using System.Runtime.CompilerServices;

namespace UraniumCompute.Backend;

/// <inheritdoc cref="MemoryMapper{T}"/>
public sealed class MemoryMapper1D<T> : MemoryMapper<T>, IReadOnlyList<T>
    where T : unmanaged
{
    public T this[int index]
    {
        get => GetElementAt(index);
        set => GetElementAt(index) = value;
    }

    internal unsafe MemoryMapper1D(in DeviceMemorySlice slice, T* map) : base(slice, map)
    {
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
    public Span<T> Slice(int start, int length)
    {
        if (start < 0 || length < 0 || (ulong)(start + length) > LongCount)
        {
            throw new IndexOutOfRangeException("Memory map out of range");
        }

        unsafe
        {
            return new Span<T>(mapPointer + start, length);
        }
    }

    internal unsafe MemoryMapper2D<T> ReshapeUnchecked(ulong width, ulong height)
    {
        return new MemoryMapper2D<T>(width, height, in memorySlice, mapPointer);
    }
}
