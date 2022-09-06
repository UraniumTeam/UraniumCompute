using System.Runtime.InteropServices;
using UraniumCompute.Acceleration;
using UraniumCompute.Memory;

namespace UraniumCompute.Backend;

public class BufferBase : DeviceObject<BufferBase.Desc>
{
    public override Desc Descriptor
    {
        get
        {
            IBuffer_GetDesc(Handle, out var value);
            return value;
        }
    }

    public DeviceMemorySlice BoundMemory { get; private set; }

    internal BufferBase(IntPtr handle) : base(handle)
    {
    }

    public override void Init(in Desc desc)
    {
        IBuffer_Init(Handle, in desc).ThrowOnError("Couldn't initialize buffer");
    }

    public DeviceMemory AllocateMemory(NativeString memoryDebugName, MemoryKindFlags flags, ulong overrideSize = ulong.MaxValue)
    {
        Span<IntPtr> handle = stackalloc IntPtr[1];
        handle[0] = Handle;
        var memorySize = overrideSize == ulong.MaxValue ? Descriptor.Size : overrideSize;
        var memory = Device.CreateMemory();
        memory.Init(memoryDebugName, memorySize, handle, flags);
        return memory;
    }

    public void BindMemory(in DeviceMemorySlice memorySlice)
    {
        TryBindMemory(in memorySlice)
            .ThrowOnError("Couldn't bind memory to buffer");
    }

    public ResultCode TryBindMemory(in DeviceMemorySlice memorySlice)
    {
        BoundMemory = memorySlice;
        var sliceNative = new DeviceMemorySliceNative(memorySlice.Memory.Handle, memorySlice.Offset, memorySlice.Size);
        return IBuffer_BindMemory(Handle, sliceNative);
    }

    [DllImport("UnCompute")]
    private static extern ResultCode IBuffer_Init(IntPtr self, in Desc desc);

    [DllImport("UnCompute")]
    private static extern void IBuffer_GetDesc(IntPtr self, out Desc desc);

    [DllImport("UnCompute")]
    private static extern ResultCode IBuffer_BindMemory(IntPtr self, in DeviceMemorySliceNative slice);

    [StructLayout(LayoutKind.Sequential)]
    private readonly record struct DeviceMemorySliceNative(IntPtr Memory, ulong Offset, ulong Size);

    /// <summary>
    ///     Buffer descriptor.
    /// </summary>
    /// <param name="Name">Debug name of the object.</param>
    /// <param name="Size">Size of the buffer.</param>
    [StructLayout(LayoutKind.Sequential)]
    public readonly record struct Desc(NativeString Name, ulong Size);
}
