using System.Runtime.InteropServices;
using UraniumCompute.Acceleration;
using UraniumCompute.Containers;
using UraniumCompute.Memory;

namespace UraniumCompute.Backend;

/// <summary>
///     A handle to backend-specific memory.
/// </summary>
public sealed class DeviceMemory : DeviceObject<DeviceMemory.Desc>
{
    public const ulong WholeSize = ulong.MaxValue;

    public override unsafe Desc Descriptor
    {
        get
        {
            IDeviceMemory_GetDesc(Handle, out var value);
            var objects = new ReadOnlySpan<IntPtr>(value.Objects.pBegin, (int)value.Objects.Length());
            return new Desc(value.Name, value.Size, objects.ToArray(), value.Flags);
        }
    }

    internal DeviceMemory(IntPtr handle) : base(handle)
    {
    }

    public override void Init(in Desc desc)
    {
        var objects = desc.Objects.ToArray();
        Init(desc.Name, desc.Size, objects, desc.Flags);
    }

    /// <summary>
    ///     A more efficient version of Init. Doesn't allocate an array of object handles.
    /// </summary>
    /// <param name="name">Debug name.</param>
    /// <param name="size">Size of allocated device memory.</param>
    /// <param name="objects">
    ///     A span of object handles (see <see cref="NativeObject.Handle" />) that the allocated memory must be compatible
    ///     with.
    /// </param>
    /// <param name="flags">Memory kind flags.</param>
    public unsafe void Init(NativeString name, ulong size, ReadOnlySpan<IntPtr> objects, MemoryKindFlags flags)
    {
        fixed (IntPtr* p = objects)
        {
            var descNative = new DescNative(name, size, new ArraySliceBase
            {
                pBegin = (sbyte*)p,
                pEnd = (sbyte*)(p + objects.Length)
            }, flags);

            IDeviceMemory_Init(Handle, in descNative).ThrowOnError("Couldn't initialize device memory");
        }
    }

    internal unsafe void* MapImpl(ulong byteOffset = 0, ulong byteSize = WholeSize)
    {
        IDeviceMemory_Map(Handle, byteOffset, byteSize, out var data).ThrowOnError("Couldn't map device memory");
        return (void*)data;
    }

    /// <summary>
    ///     Map the device memory memory to access it from the host.
    /// </summary>
    /// <param name="byteOffset">Byte offset of the memory to map.</param>
    /// <param name="byteSize">Size of the part of the memory to map.</param>
    /// <typeparam name="T"></typeparam>
    /// <returns>
    ///     A <see cref="MemoryMapHelper{T}" /> object that holds a pointer to the mapped memory.
    /// </returns>
    public unsafe MemoryMapHelper<T> Map<T>(ulong byteOffset = 0, ulong byteSize = WholeSize)
        where T : unmanaged
    {
        var ptr = (T*)MapImpl(byteOffset, byteSize);
        return new MemoryMapHelper<T>(this, byteOffset, byteSize, ptr);
    }

    /// <summary>
    ///     Unmap the mapped memory.
    /// </summary>
    public void Unmap()
    {
        IDeviceMemory_Unmap(Handle);
    }

    /// <summary>
    ///     Check if the memory is compatible with an object
    /// </summary>
    /// The implementation is backend-specific, it not only checks if the size of device memory is greater
    /// or equal to the size of memory required by the object, but also checks backend's memory type, e.g.
    /// Vulkan's memory type bits to be compatible.
    /// <param name="deviceObject">The object to check the memory for.</param>
    /// <returns>True if the memory is compatible.</returns>
    public bool IsCompatible(DeviceObject deviceObject)
    {
        return IDeviceMemory_IsCompatible(Handle, deviceObject.Handle);
    }

    [DllImport("UnCompute")]
    private static extern ResultCode IDeviceMemory_Init(IntPtr self, in DescNative desc);

    [DllImport("UnCompute")]
    private static extern void IDeviceMemory_GetDesc(IntPtr self, out DescNative desc);

    [DllImport("UnCompute")]
    private static extern ResultCode IDeviceMemory_Map(IntPtr self, ulong byteOffset, ulong byteSize, out IntPtr data);

    [DllImport("UnCompute")]
    private static extern void IDeviceMemory_Unmap(IntPtr self);

    [DllImport("UnCompute")]
    private static extern bool IDeviceMemory_IsCompatible(IntPtr self, IntPtr deviceObject);

    [StructLayout(LayoutKind.Sequential)]
    private readonly record struct DescNative(NativeString Name, ulong Size, ArraySliceBase Objects, MemoryKindFlags Flags);

    /// <summary>
    ///     Device memory descriptor.
    /// </summary>
    /// <param name="Name">Device memory debug name.</param>
    /// <param name="Size">Memory size in bytes.</param>
    /// <param name="Objects">Resource objects that the memory must be compatible with.</param>
    /// <param name="Flags">Memory kind flags.</param>
    public readonly record struct Desc(NativeString Name, ulong Size, IEnumerable<IntPtr> Objects, MemoryKindFlags Flags);
}
