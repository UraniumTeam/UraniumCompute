using System.Runtime.InteropServices;
using UraniumCompute.Backend;
using UraniumCompute.Memory;

namespace UraniumCompute.Acceleration;

public class DeviceFactory : UnObject
{
    public BackendKind BackendKind { get; }
    public IReadOnlyCollection<AdapterInfo> Adapters { get; } = Array.Empty<AdapterInfo>();

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

    [DllImport("UnCompute")]
    private static extern ResultCode CreateDeviceFactory(BackendKind backendKind, out IntPtr deviceFactory);

    [DllImport("UnCompute")]
    private static extern ResultCode IDeviceFactory_Init(IntPtr handle, in Desc desc);
}
