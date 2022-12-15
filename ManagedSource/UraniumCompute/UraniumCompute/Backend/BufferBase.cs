using System.Runtime.InteropServices;
using UraniumCompute.Acceleration;
using UraniumCompute.Memory;

namespace UraniumCompute.Backend;

/// <summary>
///     Encapsulates backend-specific buffers that store the data on the device.
/// </summary>
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

    /// <summary>
    ///     The memory bound to this buffer.
    /// </summary>
    public DeviceMemorySlice BoundMemory { get; private set; }

    internal BufferBase(IntPtr handle) : base(handle)
    {
    }

    /// <summary>
    ///     Allocate memory compatible with this buffer.
    /// </summary>
    /// <param name="memoryDebugName">Debug name that will be set to the allocated device memory.</param>
    /// <param name="flags"><see cref="MemoryKindFlags" /> to allocate the memory with.</param>
    /// <param name="overrideSize">Use the size that is not equal to the buffer's size.</param>
    /// <returns>The allocated device memory.</returns>
    public DeviceMemory AllocateMemory(NativeString memoryDebugName, MemoryKindFlags flags, ulong overrideSize = ulong.MaxValue)
    {
        ReadOnlySpan<IntPtr> handle = stackalloc IntPtr[] { Handle };
        var memorySize = overrideSize == ulong.MaxValue ? Descriptor.Size : overrideSize;
        var memory = Device.CreateMemory();
        memory.Init(new DeviceMemory.Desc(memoryDebugName, memorySize, handle, flags));
        return memory;
    }

    /// <summary>
    ///     Bind device memory slice to this buffer.
    /// </summary>
    /// <param name="memorySlice">Memory slice to bind.</param>
    public void BindMemory(in DeviceMemorySlice memorySlice)
    {
        TryBindMemory(in memorySlice)
            .ThrowOnError("Couldn't bind memory to buffer");
    }

    /// <summary>
    ///     Bind device memory to this buffer.
    /// </summary>
    /// <param name="memory">Memory to bind.</param>
    public void BindMemory(DeviceMemory memory)
    {
        TryBindMemory(new DeviceMemorySlice(memory))
            .ThrowOnError("Couldn't bind memory to buffer");
    }

    /// <summary>
    ///     Try to bind device memory slice to this buffer or get the error code.
    ///     Unlike <see cref="BindMemory" />, this function doesn't throw exceptions.
    /// </summary>
    /// <param name="memorySlice">Memory slice to bind.</param>
    /// <returns>The <see cref="ResultCode" /> of the operation.</returns>
    public ResultCode TryBindMemory(in DeviceMemorySlice memorySlice)
    {
        BoundMemory = memorySlice;
        var sliceNative = new DeviceMemorySliceNative(memorySlice.Memory.Handle, memorySlice.Offset, memorySlice.Size);
        return IBuffer_BindMemory(Handle, sliceNative);
    }

    protected override void InitInternal(in Desc desc)
    {
        IBuffer_Init(Handle, in desc).ThrowOnError("Couldn't initialize buffer");
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
    ///     Buffer usage type.
    /// </summary>
    public enum Usage
    {
        /// <summary>
        ///     The buffer is used as a storage for an array of elements.
        /// </summary>
        Storage,

        /// <summary>
        ///     The buffer is used to store kernel constants.
        /// </summary>
        Constant
    }

    /// <summary>
    ///     Buffer descriptor.
    /// </summary>
    /// <param name="Name">Debug name of the object.</param>
    /// <param name="Size">Size of the buffer.</param>
    [StructLayout(LayoutKind.Sequential)]
    public readonly record struct Desc(NativeString Name, ulong Size, Usage Usage)
    {
        public override int GetHashCode()
        {
            return HashCode.Combine(Size, Usage);
        }
    }
}
