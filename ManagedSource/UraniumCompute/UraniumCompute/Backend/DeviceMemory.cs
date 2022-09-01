using System.Runtime.InteropServices;
using UraniumCompute.Acceleration;
using UraniumCompute.Containers;
using UraniumCompute.Memory;

namespace UraniumCompute.Backend;

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

    public void Init(in Desc desc)
    {
        var objects = desc.Objects.ToArray();
        Init(desc.Name, desc.Size, objects, desc.Flags);
    }

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

    public unsafe MemoryMapHelper<T> Map<T>(ulong byteOffset = 0, ulong byteSize = WholeSize)
        where T : unmanaged
    {
        var ptr = (T*)MapImpl(byteOffset, byteSize);
        return new MemoryMapHelper<T>(this, byteOffset, byteSize, ptr);
    }

    public void Unmap()
    {
        IDeviceMemory_Unmap(Handle);
    }

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

    public readonly record struct Desc(NativeString Name, ulong Size, IEnumerable<IntPtr> Objects, MemoryKindFlags Flags);
}
