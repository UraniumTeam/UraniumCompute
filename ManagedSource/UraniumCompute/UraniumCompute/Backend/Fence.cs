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

    internal Fence(IntPtr handle) : base(handle)
    {
    }

    public override void Init(in Desc desc)
    {
        IFence_Init(Handle, in desc);
    }

    public void SignalOnCpu()
    {
        IFence_SignalOnCpu(Handle).ThrowOnError("Couldn't signal a fence");
    }

    public void ResetState()
    {
        IFence_ResetState(Handle);
    }

    public void WaitOnCpu()
    {
        IFence_WaitOnCpu(Handle).ThrowOnError("Error while waiting for a fence");
    }

    public ResultCode WaitOnCpu(ulong nanosecondTimeout)
    {
        return IFence_WaitOnCpu_Timeout(Handle, nanosecondTimeout);
    }

    public ResultCode WaitOnCpu(in TimeSpan timeout)
    {
        return IFence_WaitOnCpu_Timeout(Handle, (ulong)(timeout.TotalMilliseconds * 1_000_000));
    }

    [DllImport("UnCompute")]
    private static extern ResultCode IFence_Init(IntPtr self, in Desc desc);

    [DllImport("UnCompute")]
    private static extern void IFence_GetDesc(IntPtr self, out Desc desc);

    [DllImport("UnCompute")]
    private static extern ResultCode IFence_SignalOnCpu(IntPtr self);

    [DllImport("UnCompute")]
    private static extern ResultCode IFence_WaitOnCpu_Timeout(IntPtr self, ulong timeout);

    [DllImport("UnCompute")]
    private static extern ResultCode IFence_WaitOnCpu(IntPtr self);

    [DllImport("UnCompute")]
    private static extern void IFence_ResetState(IntPtr self);

    [DllImport("UnCompute")]
    private static extern FenceState IFence_GetState(IntPtr self);

    [StructLayout(LayoutKind.Sequential)]
    public readonly record struct Desc(NativeString Name, FenceState InitialState);
}
