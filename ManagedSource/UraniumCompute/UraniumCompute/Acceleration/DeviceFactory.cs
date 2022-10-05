using System.Runtime.InteropServices;
using UraniumCompute.Backend;
using UraniumCompute.Compilation;
using UraniumCompute.Containers;
using UraniumCompute.Memory;

namespace UraniumCompute.Acceleration;

/// <summary>
///     This class is used to create backend-specific compute devices and related objects.
/// </summary>
public sealed class DeviceFactory : NativeObject
{
    /// <summary>
    ///     Kind of backend for the compute devices created by this factory.
    /// </summary>
    public BackendKind BackendKind { get; }

    /// <summary>
    ///     All adapters supported by the specified backend.
    /// </summary>
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

    /// <summary>
    ///     Create device factory.
    /// </summary>
    /// <param name="backendKind">Kind of backend that the created factory will create objects for.</param>
    /// <returns>The created device factory.</returns>
    /// <exception cref="ErrorResultException">Unmanaged function returned an error code.</exception>
    public static DeviceFactory Create(BackendKind backendKind)
    {
        return Create(backendKind, out var deviceFactory) switch
        {
            ResultCode.Success => deviceFactory!,
            var resultCode => throw new ErrorResultException("Couldn't create DeviceFactory", resultCode)
        };
    }

    /// <summary>
    ///     Create device factory.
    /// </summary>
    /// <param name="backendKind">Kind of backend that the created factory will create objects for.</param>
    /// <param name="deviceFactory">The created device factory.</param>
    /// <returns>The <see cref="ResultCode" /> of the operation.</returns>
    public static ResultCode Create(BackendKind backendKind, out DeviceFactory? deviceFactory)
    {
        var resultCode = CreateDeviceFactory(backendKind, out var handle);
        deviceFactory = resultCode == ResultCode.Success ? new DeviceFactory(handle, backendKind) : null;
        return resultCode;
    }

    /// <summary>
    ///     Initialize device factory.
    /// </summary>
    /// <param name="desc">The device factory descriptor.</param>
    public void Init(in Desc desc)
    {
        IDeviceFactory_Init(Handle, in desc).ThrowOnError("Couldn't initialize Device factory");
    }

    /// <summary>
    ///     Create a compute device.
    /// </summary>
    /// <returns>The created device</returns>
    /// <exception cref="ErrorResultException">Unmanaged function returned an error code.</exception>
    public ComputeDevice CreateDevice()
    {
        return CreateDevice(out var device) switch
        {
            ResultCode.Success => device!,
            var resultCode => throw new ErrorResultException("Couldn't create Device", resultCode)
        };
    }

    /// <summary>
    ///     Create a kernel compiler.
    /// </summary>
    /// <returns>The created compiler</returns>
    /// <exception cref="ErrorResultException">Unmanaged function returned an error code.</exception>
    public KernelCompiler CreateKernelCompiler()
    {
        return CreateKernelCompiler(out var compiler) switch
        {
            ResultCode.Success => compiler!,
            var resultCode => throw new ErrorResultException("Couldn't create Kernel compiler", resultCode)
        };
    }

    private ResultCode CreateDevice(out ComputeDevice? computeDevice)
    {
        var resultCode = IDeviceFactory_CreateDevice(Handle, out var device);
        computeDevice = resultCode == ResultCode.Success ? new ComputeDevice(device) : null;
        return resultCode;
    }

    private ResultCode CreateKernelCompiler(out KernelCompiler? kernelCompiler)
    {
        var resultCode = IDeviceFactory_CreateKernelCompiler(Handle, out var compiler);
        kernelCompiler = resultCode == ResultCode.Success ? new KernelCompiler(compiler) : null;
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

    [DllImport("UnCompute")]
    private static extern ResultCode IDeviceFactory_CreateKernelCompiler(IntPtr self, out IntPtr compiler);

    /// <summary>
    ///     Device factory descriptor.
    /// </summary>
    /// <param name="ApplicationName">Name of the user application.</param>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public readonly record struct Desc(string ApplicationName);
}
