using System.Runtime.InteropServices;
using UraniumCompute.Acceleration;
using UraniumCompute.Memory;

namespace UraniumCompute.Backend;

/// <summary>
///     GPU synchronization primitives that can be either signaled or reset.
/// </summary>
public sealed class Fence : DeviceObject<Fence.Desc>
{
    public override Desc Descriptor
    {
        get
        {
            IFence_GetDesc(Handle, out var value);
            return value;
        }
    }

    /// <summary>
    ///     Signal, reset the fence or get its state.
    /// </summary>
    public FenceState State
    {
        get => IFence_GetState(Handle);
        set
        {
            if (value == FenceState.Reset)
            {
                IFence_ResetState(Handle);
            }
            else
            {
                IFence_SignalOnCpu(Handle).ThrowOnError("Fence state change failed");
            }
        }
    }

    internal Fence(nint handle) : base(handle)
    {
    }

    /// <summary>
    ///     Signal the fence.
    /// </summary>
    public void SignalOnCpu()
    {
        IFence_SignalOnCpu(Handle).ThrowOnError("Couldn't signal a fence");
    }

    /// <summary>
    ///     Reset fence's state.
    /// </summary>
    public void ResetState()
    {
        IFence_ResetState(Handle);
    }

    /// <summary>
    ///     Wait for the fence to signal.
    /// </summary>
    public void WaitOnCpu()
    {
        IFence_WaitOnCpu(Handle).ThrowOnError("Error while waiting for a fence");
    }

    /// <summary>
    ///     Wait for the fence to signal.
    /// </summary>
    /// <param name="nanosecondTimeout">Timeout in nanoseconds.</param>
    /// <returns><see cref="ResultCode.Success" /> or <see cref="ResultCode.Timeout" /></returns>
    public ResultCode WaitOnCpu(ulong nanosecondTimeout)
    {
        return IFence_WaitOnCpu_Timeout(Handle, nanosecondTimeout);
    }

    /// <summary>
    ///     Wait for the fence to signal.
    /// </summary>
    /// <param name="timeout">Timeout as a <see cref="TimeSpan" />.</param>
    /// <returns><see cref="ResultCode.Success" /> or <see cref="ResultCode.Timeout" /></returns>
    public ResultCode WaitOnCpu(in TimeSpan timeout)
    {
        return IFence_WaitOnCpu_Timeout(Handle, (ulong)(timeout.TotalMilliseconds * 1_000_000));
    }

    protected override void InitInternal(in Desc desc)
    {
        IFence_Init(Handle, in desc);
    }

    [DllImport("UnCompute")]
    private static extern ResultCode IFence_Init(nint self, in Desc desc);

    [DllImport("UnCompute")]
    private static extern void IFence_GetDesc(nint self, out Desc desc);

    [DllImport("UnCompute")]
    private static extern ResultCode IFence_SignalOnCpu(nint self);

    [DllImport("UnCompute")]
    private static extern ResultCode IFence_WaitOnCpu_Timeout(nint self, ulong timeout);

    [DllImport("UnCompute")]
    private static extern ResultCode IFence_WaitOnCpu(nint self);

    [DllImport("UnCompute")]
    private static extern void IFence_ResetState(nint self);

    [DllImport("UnCompute")]
    private static extern FenceState IFence_GetState(nint self);

    /// <summary>
    ///     Fence descriptor.
    /// </summary>
    /// <param name="Name">Fence debug name.</param>
    /// <param name="InitialState">Fence initial state.</param>
    [StructLayout(LayoutKind.Sequential)]
    public readonly record struct Desc(NativeString Name, FenceState InitialState) : IDeviceObjectDescriptor;
}
