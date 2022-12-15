using System.Runtime.InteropServices;
using UraniumCompute.Acceleration;
using UraniumCompute.Common.Math;
using UraniumCompute.Memory;

namespace UraniumCompute.Backend;

/// <summary>
///     Command lists record commands to be executed by the backend.
/// </summary>
public sealed class CommandList : DeviceObject<CommandList.Desc>
{
    public override Desc Descriptor
    {
        get
        {
            ICommandList_GetDesc(Handle, out var desc);
            return desc;
        }
    }

    /// <summary>
    ///     Get command list state.
    /// </summary>
    public CommandListState State => ICommandList_GetState(Handle);

    /// <summary>
    ///     The fence that is signaled after submit operation is complete.
    /// </summary>
    /// <exception cref="InvalidOperationException">The command list was uninitialized</exception>
    public Fence CompletionFence => fence ?? throw new InvalidOperationException("The command list was uninitialized");

    private Fence? fence;

    internal CommandList(IntPtr handle) : base(handle)
    {
    }

    /// <summary>
    ///     Set the command list state to the Recording state.
    /// </summary>
    /// <returns>Command list recording context.</returns>
    public ICommandRecordingContext Begin()
    {
        return new Builder(Handle);
    }

    /// <summary>
    ///     Set the command list state to the Initial state.
    /// </summary>
    public void ResetState()
    {
        ICommandList_ResetState(Handle);
    }

    /// <summary>
    ///     Submit the command list and set the state to the Pending state.
    /// </summary>
    public void Submit()
    {
        ICommandList_Submit(Handle).ThrowOnError("Couldn't submit command list for execution");
    }

    protected override void InitInternal(in Desc desc)
    {
        ICommandList_Init(Handle, in desc);
        var fenceHandle = ICommandList_GetFence(Handle);
        fence = new Fence(fenceHandle);
    }

    [DllImport("UnCompute")]
    private static extern ResultCode ICommandList_Init(IntPtr self, in Desc desc);

    [DllImport("UnCompute")]
    private static extern void ICommandList_GetDesc(IntPtr self, out Desc desc);

    [DllImport("UnCompute")]
    private static extern IntPtr ICommandList_GetFence(IntPtr self);

    [DllImport("UnCompute")]
    private static extern CommandListState ICommandList_GetState(IntPtr self);

    [DllImport("UnCompute")]
    private static extern bool ICommandList_Begin(IntPtr self, out NativeBuilder builder);

    [DllImport("UnCompute")]
    private static extern void ICommandList_ResetState(IntPtr self);

    [DllImport("UnCompute")]
    private static extern ResultCode ICommandList_Submit(IntPtr self);

    [StructLayout(LayoutKind.Sequential)]
    private readonly struct NativeBuilder
    {
        private readonly IntPtr commandListHandle;
    }

    private sealed class Builder : ICommandRecordingContext
    {
        private NativeBuilder builder;

        internal Builder(IntPtr commandListHandle)
        {
            if (!ICommandList_Begin(commandListHandle, out builder))
            {
                throw new InvalidOperationException("Couldn't begin command list recording");
            }
        }

        public void MemoryBarrierUnsafe(BufferBase buffer, in MemoryBarrierDesc barrierDesc)
        {
            CommandListBuilder_MemoryBarrier(ref builder, buffer.Handle, in barrierDesc);
        }

        public void CopyUnsafe(BufferBase source, BufferBase destination, in BufferCopyRegion region)
        {
            CommandListBuilder_Copy(ref builder, source.Handle, destination.Handle, in region);
        }

        public void Dispatch(Kernel kernel, int x, int y, int z)
        {
            CommandListBuilder_Dispatch(ref builder, kernel.Handle, x, y, z);
        }

        public void End()
        {
            CommandListBuilder_End(ref builder);
        }

        public void Dispose()
        {
            End();
        }

        [DllImport("UnCompute")]
        private static extern void CommandListBuilder_End(ref NativeBuilder self);

        [DllImport("UnCompute")]
        private static extern void CommandListBuilder_MemoryBarrier(ref NativeBuilder self, IntPtr buffer,
            in MemoryBarrierDesc barrierDesc);

        [DllImport("UnCompute")]
        private static extern void CommandListBuilder_Copy(ref NativeBuilder self, IntPtr source, IntPtr destination,
            in BufferCopyRegion region);

        [DllImport("UnCompute")]
        private static extern void CommandListBuilder_Dispatch(ref NativeBuilder self, IntPtr kernel, int x, int y, int z);
    }

    /// <summary>
    ///     Command list descriptor.
    /// </summary>
    /// <param name="Name">Command list debug name.</param>
    /// <param name="QueueKindFlags">Command queue kind flags.</param>
    /// <param name="Flags">Command list flags.</param>
    [StructLayout(LayoutKind.Sequential)]
    public readonly record struct Desc(NativeString Name, HardwareQueueKindFlags QueueKindFlags,
        CommandListFlags Flags = CommandListFlags.None);
}
