using System.Runtime.InteropServices;
using UraniumCompute.Acceleration;
using UraniumCompute.Memory;

namespace UraniumCompute.Backend;

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

    public CommandListState State => ICommandList_GetState(Handle);

    public Fence CompletionFence => fence ?? throw new InvalidOperationException("The command list was uninitialized");

    private Fence? fence;

    public CommandList(IntPtr handle) : base(handle)
    {
    }

    public override void Init(in Desc desc)
    {
        ICommandList_Init(Handle, in desc);
        var fenceHandle = ICommandList_GetFence(Handle);
        fence = new Fence(fenceHandle);
    }

    public Builder Begin()
    {
        return new Builder(Handle);
    }

    public void ResetState()
    {
        ICommandList_ResetState(Handle);
    }

    public void Submit()
    {
        ICommandList_Submit(Handle).ThrowOnError("Couldn't submit command list for execution");
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

    public sealed class Builder : IDisposable
    {
        private NativeBuilder builder;

        internal Builder(IntPtr commandListHandle)
        {
            if (!ICommandList_Begin(commandListHandle, out builder))
            {
                throw new InvalidOperationException("Couldn't begin command list recording");
            }
        }

        public void Copy<T>(Buffer<T> source, Buffer<T> destination)
            where T : unmanaged
        {
            if (source.Descriptor.Size != destination.Descriptor.Size)
            {
                throw new InvalidOperationException("Source and destination sizes must be equal");
            }

            CommandListBuilder_Copy(ref builder, source.Handle, destination.Handle, new BufferCopyRegion(source.Descriptor.Size));
        }

        public void Copy<T>(Buffer<T> source, Buffer<T> destination, in BufferCopyRegion region)
            where T : unmanaged
        {
            CommandListBuilder_Copy(ref builder, source.Handle, destination.Handle, in region);
        }

        public void CopyUnsafe(BufferBase source, BufferBase destination, in BufferCopyRegion region)
        {
            CommandListBuilder_Copy(ref builder, source.Handle, destination.Handle, in region);
        }

        public void Dispose()
        {
            CommandListBuilder_End(ref builder);
        }

        [DllImport("UnCompute")]
        private static extern void CommandListBuilder_End(ref NativeBuilder self);

        [DllImport("UnCompute")]
        private static extern void CommandListBuilder_Copy(ref NativeBuilder self, IntPtr pSource, IntPtr pDestination,
            in BufferCopyRegion region);
    }

    [StructLayout(LayoutKind.Sequential)]
    public readonly record struct Desc(NativeString Name, HardwareQueueKindFlags QueueKindFlags,
        CommandListFlags Flags = CommandListFlags.None);
}
