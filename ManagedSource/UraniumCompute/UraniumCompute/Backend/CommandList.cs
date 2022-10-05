﻿using System.Runtime.InteropServices;
using UraniumCompute.Acceleration;
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

    public override void Init(in Desc desc)
    {
        ICommandList_Init(Handle, in desc);
        var fenceHandle = ICommandList_GetFence(Handle);
        fence = new Fence(fenceHandle);
    }

    /// <summary>
    ///     Set the command list state to the Recording state.
    /// </summary>
    /// <returns>Command list builder object.</returns>
    public Builder Begin()
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

    /// <summary>
    ///     Command list builder that is used to record device commands.
    /// </summary>
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

        /// <summary>
        ///     Insert a memory dependency.
        /// </summary>
        /// <param name="buffer">The buffer affected by the barrier.</param>
        /// <param name="barrierDesc">The barrier descriptor.</param>
        /// <typeparam name="T">Type of the elements stored in the buffer affected by the barrier.</typeparam>
        public void MemoryBarrier<T>(Buffer<T> buffer, in MemoryBarrierDesc barrierDesc)
            where T : unmanaged
        {
            CommandListBuilder_MemoryBarrier(ref builder, buffer.Handle, in barrierDesc);
        }

        /// <summary>
        ///     Insert a memory dependency.
        /// </summary>
        /// <param name="buffer">The buffer affected by the barrier.</param>
        /// <param name="sourceAccess">Source access mask.</param>
        /// <param name="destAccess">Destination access mask.</param>
        /// <typeparam name="T">Type of the elements stored in the buffer affected by the barrier.</typeparam>
        public void MemoryBarrier<T>(Buffer<T> buffer, AccessFlags sourceAccess, AccessFlags destAccess)
            where T : unmanaged
        {
            MemoryBarrier(buffer, new MemoryBarrierDesc(sourceAccess, destAccess));
        }

        /// <summary>
        ///     Copy a region of the source buffer to the destination buffer.
        /// </summary>
        /// <param name="source">Source buffer.</param>
        /// <param name="destination">Destination buffer.</param>
        /// <typeparam name="T">Type of the elements stored in the buffers.</typeparam>
        /// <exception cref="InvalidOperationException">Source and destination sizes where not equal.</exception>
        public void Copy<T>(Buffer<T> source, Buffer<T> destination)
            where T : unmanaged
        {
            if (source.Descriptor.Size != destination.Descriptor.Size)
            {
                throw new InvalidOperationException("Source and destination sizes must be equal");
            }

            CommandListBuilder_Copy(ref builder, source.Handle, destination.Handle, new BufferCopyRegion(source.Descriptor.Size));
        }

        /// <summary>
        ///     Copy a region of the source buffer to the destination buffer.
        /// </summary>
        /// <param name="source">Source buffer.</param>
        /// <param name="destination">Destination buffer.</param>
        /// <param name="region">Copy region.</param>
        /// <typeparam name="T">Type of the elements stored in the buffers.</typeparam>
        public void Copy<T>(Buffer<T> source, Buffer<T> destination, in BufferCopyRegion region)
            where T : unmanaged
        {
            CommandListBuilder_Copy(ref builder, source.Handle, destination.Handle, in region);
        }

        /// <summary>
        ///     A not type-safe version of buffer copy command (<see cref="Copy{T}(Buffer{T},Buffer{T})" />).
        /// </summary>
        /// <param name="source">Source buffer.</param>
        /// <param name="destination">Destination buffer.</param>
        /// <param name="region">Copy region.</param>
        public void CopyUnsafe(BufferBase source, BufferBase destination, in BufferCopyRegion region)
        {
            CommandListBuilder_Copy(ref builder, source.Handle, destination.Handle, in region);
        }

        /// <summary>
        ///     Dispatch a compute kernel to execute on the device.
        /// </summary>
        /// <param name="kernel">The kernel to dispatch.</param>
        /// <param name="x">The number of local workgroups to dispatch in the X dimension.</param>
        /// <param name="y">The number of local workgroups to dispatch in the Y dimension.</param>
        /// <param name="z">The number of local workgroups to dispatch in the Z dimension.</param>
        public void Dispatch(Kernel kernel, int x, int y, int z)
        {
            CommandListBuilder_Dispatch(ref builder, kernel.Handle, x, y, z);
        }

        /// <summary>
        ///     Set the command list state to Executable and end command recording.
        /// </summary>
        public void Dispose()
        {
            CommandListBuilder_End(ref builder);
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

    [StructLayout(LayoutKind.Sequential)]
    public readonly record struct Desc(NativeString Name, HardwareQueueKindFlags QueueKindFlags,
        CommandListFlags Flags = CommandListFlags.None);
}
