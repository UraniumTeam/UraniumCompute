using System.Runtime.InteropServices;
using UraniumCompute.Acceleration;
using UraniumCompute.Containers;
using UraniumCompute.Memory;

namespace UraniumCompute.Backend;

public sealed class DeviceMemory : DeviceObject<DeviceMemory.Desc>
{
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

    public unsafe void Init(in Desc desc)
    {
        var objects = desc.Objects.ToArray();
        Init(desc.Name, desc.Size, objects, desc.Flags);
    }

    public unsafe void Init(NativeString name, long size, ReadOnlySpan<IntPtr> objects, MemoryKindFlags flags)
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

    public unsafe void* Map(long byteOffset = 0, long byteSize = long.MaxValue)
    {
        IDeviceMemory_Map(Handle, byteOffset, byteSize, out var data).ThrowOnError("Couldn't map device memory");
        return (void*)data;
    }

    public unsafe T* Map<T>(long byteOffset = 0, long byteSize = long.MaxValue)
        where T : unmanaged
    {
        return (T*)Map(byteOffset, byteSize);
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
    private static extern ResultCode IDeviceMemory_Map(IntPtr self, long byteOffset, long byteSize, out IntPtr data);

    [DllImport("UnCompute")]
    private static extern void IDeviceMemory_Unmap(IntPtr self);

    [DllImport("UnCompute")]
    private static extern bool IDeviceMemory_IsCompatible(IntPtr self, IntPtr deviceObject);

    [StructLayout(LayoutKind.Sequential)]
    private readonly record struct DescNative(NativeString Name, long Size, ArraySliceBase Objects, MemoryKindFlags Flags);

    public readonly record struct Desc(NativeString Name, long Size, IEnumerable<IntPtr> Objects, MemoryKindFlags Flags);
}
