using System.Runtime.InteropServices;
using UraniumCompute.Acceleration;
using UraniumCompute.Memory;

namespace UraniumCompute.Backend;

/// <summary>Interface for all backend-specific compute devices.</summary>
/// Compute device is an object that allows users to create data buffers, synchronization primitives and other objects
/// using the target backend. It allows to run compute kernels using ICommandList interface.
/// 
/// Compute devices are a part of UraniumCompute low-level API and should not be used directly together with job graphs
/// and other higher-level objects.
public sealed class ComputeDevice : UnObject
{
    internal ComputeDevice(IntPtr handle) : base(handle)
    {
    }

    /// <summary>
    ///     Initializes the compute device with the provided descriptor.
    /// </summary>
    /// <param name="desc">The compute device descriptor.</param>
    /// <exception cref="ErrorResultException">Unmanaged function returned an error code.</exception>
    public void Init(in Desc desc)
    {
        IComputeDevice_Init(Handle, in desc).ThrowOnError("Couldn't initialize Compute device");
    }

    [DllImport("UnCompute")]
    private static extern ResultCode IComputeDevice_Init(IntPtr self, in Desc desc);

    [DllImport("UnCompute")]
    private static extern ResultCode IComputeDevice_CreateBuffer(IntPtr self, out IntPtr buffer);

    [DllImport("UnCompute")]
    private static extern ResultCode IComputeDevice_CreateMemory(IntPtr self, out IntPtr buffer);

    /// <summary>
    ///     Compute device descriptor.
    /// </summary>
    /// <param name="AdapterId">ID of the adapter to create the device on.</param>
    [StructLayout(LayoutKind.Sequential)]
    public readonly record struct Desc(int AdapterId);
}
