using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace UraniumCompute.Containers;

/// <summary>
///     An array stored in unmanaged memory.
/// </summary>
/// <typeparam name="T">Type of array element, must be unmanaged.</typeparam>
[StructLayout(LayoutKind.Sequential)]
public struct NativeArray<T> : IReadOnlyList<T>, IDisposable
    where T : unmanaged
{
    public readonly int Count => (int)LongCount;

    /// <summary>
    ///     Number of elements in the array.
    /// </summary>
    public readonly unsafe long LongCount => native.Storage.Length() / sizeof(T);

    internal unsafe T* NativePointer => (T*)native.Storage.pBegin;

    /// <summary>
    ///     Get or set an element of array by index.
    /// </summary>
    /// <param name="index">The zero-based index of the element.</param>
    public T this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetElementAt(index);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => GetElementAt(index) = value;
    }

    private NativeArrayBase native;

    /// <summary>
    ///     Allocate an array with <see cref="size" /> elements. Memory stored in the array is undefined.
    /// </summary>
    /// <param name="size">Number of array elements.</param>
    public unsafe NativeArray(long size)
    {
        NativeArrayBase.HeapArray_CreateWithSize((ulong)(size * sizeof(T)), out native);
    }

    /// <summary>
    ///     Allocate an array and copy the data from <see cref="span" /> to the allocated memory.
    /// </summary>
    /// <param name="span">The span to copy the data from.</param>
    public unsafe NativeArray(ReadOnlySpan<T> span)
    {
        NativeArrayBase.HeapArray_CreateWithSize((ulong)(span.Length * sizeof(T)), out native);
        span.CopyTo(this[..]);
    }

    internal NativeArray(in NativeArrayBase arrayBase)
    {
        native = arrayBase;
    }

    public readonly IEnumerator<T> GetEnumerator()
    {
        for (long i = 0; i < LongCount; ++i)
        {
            yield return GetElementAtUnchecked(i);
        }
    }

    /// <summary>
    ///     Resize the array. Keeps old elements and adds zeros if <see cref="size" /> is greater than the current size.
    ///     Removes the elements with greater indices if <see cref="size" /> is less.
    /// </summary>
    /// <param name="size">New array size.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe void Resize(long size)
    {
        NativeArrayBase.HeapArray_Resize(ref native, (ulong)(size * sizeof(T)));
    }

    /// <summary>
    ///     Get element at <see cref="index" /> by reference.
    /// </summary>
    /// <param name="index">The zero-based index of the element.</param>
    /// <returns>The reference of the element.</returns>
    /// <exception cref="IndexOutOfRangeException">Index was out range.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ref T GetElementAt(long index)
    {
        if (index >= LongCount)
        {
            throw new IndexOutOfRangeException();
        }

        return ref GetElementAtUnchecked(index);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Span<T> Slice(int start, int length)
    {
        return native.Storage.AsSpan<T>()[start..length];
    }

    /// <summary>
    ///     Copy the data in this array to a span.
    /// </summary>
    /// <param name="destination">The span to copy the data to.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly unsafe void CopyTo(Span<T> destination)
    {
        fixed (T* ptr = destination)
        {
            var slice = new ArraySliceBase
            {
                pBegin = (sbyte*)ptr,
                pEnd = (sbyte*)ptr + native.Storage.Length()
            };
            NativeArrayBase.HeapArray_CopyDataTo(in native, ref slice);
        }
    }

    /// <summary>
    ///     Free unmanaged memory.
    /// </summary>
    public void Dispose()
    {
        NativeArrayBase.HeapArray_Destroy(ref native);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private readonly unsafe ref T GetElementAtUnchecked(long index)
    {
        return ref ((T*)native.Storage.pBegin)[index];
    }
}
