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

    public override Desc Descriptor
    {
        get
        {
            IDeviceMemory_GetDesc(Handle, out var value);
            return value;
        }
    }

    internal DeviceMemory(IntPtr handle) : base(handle)
    {
    }

    protected override void InitInternal(in Desc desc)
    {
        IDeviceMemory_Init(Handle, in desc);
    }

    public void Init(NativeString name, ulong size, IEnumerable<DeviceObject> objects, MemoryKindFlags flags)
    {
        var handles = objects.Select(x => x.Handle).ToArray();
        Init(new Desc(name, size, handles, flags));
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
    ///     A <see cref="MemoryMapper1D{T}" /> object that holds a pointer to the mapped memory.
    /// </returns>
    public unsafe MemoryMapper1D<T> Map<T>(ulong byteOffset = 0, ulong byteSize = WholeSize)
        where T : unmanaged
    {
        var ptr = (T*)MapImpl(byteOffset, byteSize);
        return new MemoryMapper1D<T>(new DeviceMemorySlice(this, byteOffset, byteSize), ptr);
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
    private static extern ResultCode IDeviceMemory_Init(IntPtr self, in Desc desc);

    [DllImport("UnCompute")]
    private static extern void IDeviceMemory_GetDesc(IntPtr self, out Desc desc);

    [DllImport("UnCompute")]
    private static extern ResultCode IDeviceMemory_Map(IntPtr self, ulong byteOffset, ulong byteSize, out IntPtr data);

    [DllImport("UnCompute")]
    private static extern void IDeviceMemory_Unmap(IntPtr self);

    [DllImport("UnCompute")]
    private static extern bool IDeviceMemory_IsCompatible(IntPtr self, IntPtr deviceObject);

    /// <summary>
    ///     Device memory descriptor.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Desc
    {
        /// <summary>Device memory debug name.</summary>
        public NativeString Name { get; init; }

        /// <summary>Memory size in bytes.</summary>
        public ulong Size { get; init; }

        /// <summary>Resource objects that the memory must be compatible with.</summary>
        public ReadOnlySpan<IntPtr> Objects
        {
            get => objects.AsSpan<IntPtr>();
            init
            {
                unsafe
                {
                    fixed (IntPtr* p = value)
                    {
                        objects = new ArraySliceBase
                        {
                            pBegin = (sbyte*)p,
                            pEnd = (sbyte*)(p + value.Length)
                        };
                    }
                }
            }
        }

        /// <summary>Memory kind flags.</summary>
        public MemoryKindFlags Flags { get; init; }

        private readonly ArraySliceBase objects;

        /// <summary>
        ///     Device memory descriptor.
        /// </summary>
        /// <param name="name">Device memory debug name.</param>
        /// <param name="size">Memory size in bytes.</param>
        /// <param name="objects">Resource objects that the memory must be compatible with.</param>
        /// <param name="flags">Memory kind flags.</param>
        public Desc(NativeString name, ulong size, ReadOnlySpan<IntPtr> objects, MemoryKindFlags flags)
        {
            Name = name;
            Size = size;
            Flags = flags;
            this.objects = new ArraySliceBase();
            Objects = objects;
        }
    }
}
