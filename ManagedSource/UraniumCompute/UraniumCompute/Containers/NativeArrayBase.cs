using System.Runtime.InteropServices;

namespace UraniumCompute.Containers;

[StructLayout(LayoutKind.Sequential)]
internal unsafe struct NativeArrayBase
{
    public ArraySliceBase Storage;
    public void* pAllocator;

    [DllImport("UnCompute")]
    public static extern void HeapArray_Destroy(ref NativeArrayBase self);

    [DllImport("UnCompute")]
    public static extern void HeapArray_CreateWithSize(ulong size, out NativeArrayBase array);

    [DllImport("UnCompute")]
    public static extern void HeapArray_Resize(ref NativeArrayBase self, ulong size);

    [DllImport("UnCompute")]
    public static extern ulong HeapArray_CopyDataTo(in NativeArrayBase self, ref ArraySliceBase slice);
}
