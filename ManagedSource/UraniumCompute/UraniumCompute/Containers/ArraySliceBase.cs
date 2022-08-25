using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace UraniumCompute.Containers;

[StructLayout(LayoutKind.Sequential)]
internal unsafe struct ArraySliceBase
{
    public sbyte* pBegin;
    public sbyte* pEnd;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly long Length()
    {
        return pEnd - pBegin;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Span<T> AsSpan<T>()
    {
        return new Span<T>(pBegin, (int)Length());
    }
}
