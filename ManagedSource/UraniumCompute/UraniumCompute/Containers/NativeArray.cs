using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace UraniumCompute.Containers;

[StructLayout(LayoutKind.Sequential)]
public struct NativeArray<T> : IReadOnlyList<T>, IDisposable
    where T : unmanaged
{
    private NativeArrayBase native;

    public unsafe NativeArray(long size)
    {
        NativeArrayBase.HeapArray_CreateWithSize((ulong)(size * sizeof(T)), out native);
    }

    public unsafe NativeArray(Span<T> span)
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

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public readonly int Count => (int)LongCount;

    public readonly unsafe long LongCount => native.Storage.Length() / sizeof(T);

    public T this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetElementAt(index);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => GetElementAt(index) = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe void Resize(long size)
    {
        NativeArrayBase.HeapArray_Resize(ref native, (ulong)(size * sizeof(T)));
    }

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private readonly unsafe ref T GetElementAtUnchecked(long index)
    {
        return ref ((T*)native.Storage.pBegin)[index];
    }

    public void Dispose()
    {
        NativeArrayBase.HeapArray_Destroy(ref native);
    }
}
