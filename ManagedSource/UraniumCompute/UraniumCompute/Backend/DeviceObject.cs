using System.Runtime.InteropServices;
using UraniumCompute.Memory;
using UraniumCompute.Utils;

namespace UraniumCompute.Backend;

public abstract class DeviceObject : NativeObject
{
    /// <summary>
    ///     The device this object was created on.
    /// </summary>
    /// <exception cref="Exception">Device of this object was not registered</exception>
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
                // TODO: create an exception type for this
                throw new Exception("Device of this object was not registered");
            }

            device = d;
            return device;
        }
    }

    /// <summary>
    ///     Object's debug name.
    /// </summary>
    public NativeString DebugName => IDeviceObject_GetDebugName(Handle);

    private ComputeDevice? device;

    protected DeviceObject(IntPtr handle) : base(handle)
    {
    }

    /// <summary>
    ///     Reset the object to uninitialized state.
    /// </summary>
    public void Reset()
    {
        IDeviceObject_Reset(Handle);
    }

    public override string ToString()
    {
        return $"{{ {GetType().GetCSharpName()} named \"{DebugName}\": {nameof(DeviceObject)} at 0x{Handle:x} }}";
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
    private bool isInitialized;
    public abstract TDesc Descriptor { get; }

    public bool IsInitialized
    {
        get => isInitialized;
        protected set
        {
            if (isInitialized && value)
            {
                throw new InvalidOperationException("The object was already initialized");
            }

            isInitialized = value;
        }
    }

    protected DeviceObject(IntPtr handle) : base(handle)
    {
    }

    public void Init(TDesc desc)
    {
        IsInitialized = true;
        InitInternal(desc);
    }

    /// <summary>
    ///     Initialize the device object.
    /// </summary>
    /// <param name="desc">Device object descriptor.</param>
    protected abstract void InitInternal(in TDesc desc);
}
