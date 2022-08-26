using System.Runtime.InteropServices;
using UraniumCompute.Backend;
using UraniumCompute.Containers;
using UraniumCompute.Memory;

namespace UraniumCompute.Acceleration;

public sealed class DeviceFactory : UnObject
{
    public BackendKind BackendKind { get; }

    public ReadOnlySpan<AdapterInfo> Adapters
    {
        get
        {
            if (!adapters.Any())
            {
                IDeviceFactory_EnumerateAdapters(Handle, out adapters);
            }

            return adapters[..];
        }
    }

    private NativeArray<AdapterInfo> adapters;

    private DeviceFactory(IntPtr handle, BackendKind backendKind)
        : base(handle)
    {
        BackendKind = backendKind;
    }

    public static DeviceFactory Create(BackendKind backendKind)
    {
        return Create(backendKind, out var deviceFactory) switch
        {
            ResultCode.Success => deviceFactory!,
            var resultCode => throw new ErrorResultException("Couldn't create DeviceFactory", resultCode)
        };
    }

    public static ResultCode Create(BackendKind backendKind, out DeviceFactory? deviceFactory)
    {
        var resultCode = CreateDeviceFactory(backendKind, out var handle);
        deviceFactory = resultCode == ResultCode.Success ? new DeviceFactory(handle, backendKind) : null;
        return resultCode;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public readonly record struct Desc(string ApplicationName);

    public ResultCode Init(in Desc desc)
    {
        return IDeviceFactory_Init(Handle, in desc);
    }

    public ComputeDevice CreateDevice()
    {
        return CreateDevice(out var device) switch
        {
            ResultCode.Success => device!,
            var resultCode => throw new ErrorResultException("Couldn't create Device", resultCode)
        };
    }

    public ResultCode CreateDevice(out ComputeDevice? computeDevice)
    {
        var resultCode = IDeviceFactory_CreateDevice(Handle, out var device);
        computeDevice = resultCode == ResultCode.Success ? new ComputeDevice(device) : null;
        return resultCode;
    }

    [DllImport("UnCompute")]
    private static extern ResultCode CreateDeviceFactory(BackendKind backendKind, out IntPtr deviceFactory);

    [DllImport("UnCompute")]
    private static extern ResultCode IDeviceFactory_Init(IntPtr handle, in Desc desc);

    [DllImport("UnCompute")]
    private static extern void IDeviceFactory_EnumerateAdapters(IntPtr handle, out NativeArray<AdapterInfo> adapters);

    [DllImport("UnCompute")]
    private static extern ResultCode IDeviceFactory_CreateDevice(IntPtr self, out IntPtr device);
}
