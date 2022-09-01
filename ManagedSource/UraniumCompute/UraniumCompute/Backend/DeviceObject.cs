using System.Runtime.InteropServices;
using UraniumCompute.Memory;

namespace UraniumCompute.Backend;

public abstract class DeviceObject : NativeObject
{
    public ComputeDevice Device
    {
        get
        {
            if (device != null)
            {
                return device;
            }

            if (!ComputeDevice.TryGetDevice(IDeviceObject_GetDevice(Handle), out var d))
            {
                throw new Exception("Device of this object was not registered");
            }

            device = d;
            return device;
        }
    }

    public NativeString DebugName => IDeviceObject_GetDebugName(Handle);

    private ComputeDevice? device;

    protected DeviceObject(IntPtr handle) : base(handle)
    {
    }

    public void Reset()
    {
        IDeviceObject_Reset(Handle);
    }

    [DllImport("UnCompute")]
    private static extern void IDeviceObject_Reset(IntPtr self);

    [DllImport("UnCompute")]
    private static extern IntPtr IDeviceObject_GetDevice(IntPtr self);

    [DllImport("UnCompute")]
    private static extern NativeString IDeviceObject_GetDebugName(IntPtr self);
}

public abstract class DeviceObject<TDesc> : DeviceObject
{
    public abstract TDesc Descriptor { get; }

    protected DeviceObject(IntPtr handle) : base(handle)
    {
    }
}
