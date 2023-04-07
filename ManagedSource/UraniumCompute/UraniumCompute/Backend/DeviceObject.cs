using System.Runtime.InteropServices;
using UraniumBackend;
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

    protected DeviceObject(nint handle) : base(handle)
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
    private static extern void IDeviceObject_Reset(nint self);

    [DllImport("UnCompute")]
    private static extern nint IDeviceObject_GetDevice(nint self);

    [DllImport("UnCompute")]
    private static extern NativeString IDeviceObject_GetDebugName(nint self);
}

public abstract class DeviceObject<TDesc> : DeviceObject
    where TDesc : unmanaged, IDeviceObjectDescriptor
{
    /// <summary>
    ///     Device object descriptor.
    /// </summary>
    public abstract TDesc Descriptor { get; }

    /// <summary>
    ///     True if <see cref="Init" /> was called for this object.
    /// </summary>
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

    private bool isInitialized;

    protected DeviceObject(nint handle) : base(handle)
    {
    }

    /// <summary>
    ///     Initialize the device object.
    /// </summary>
    /// <param name="desc">Device object descriptor.</param>
    public void Init(TDesc desc)
    {
        using var scope = ProfilerScope.Begin($"Init \"{desc.Name}\"");
        IsInitialized = true;
        InitInternal(desc);
    }

    protected abstract void InitInternal(in TDesc desc);
}
