using UraniumCompute.Acceleration;
using UraniumCompute.Common.Math;

namespace UraniumCompute.Backend;

/// <summary>
///     Command recording context, used to record device commands.
/// </summary>
public interface ICommandRecordingContext : IDisposable
{
    /// <summary>
    ///     Insert a memory dependency.
    /// </summary>
    /// <param name="buffer">The buffer affected by the barrier.</param>
    /// <param name="barrierDesc">The barrier descriptor.</param>
    /// <typeparam name="T">Type of the elements stored in the buffer affected by the barrier.</typeparam>
    void MemoryBarrier<T>(Buffer<T> buffer, in MemoryBarrierDesc barrierDesc)
        where T : unmanaged
    {
        MemoryBarrierUnsafe(buffer, in barrierDesc);
    }

    /// <summary>
    ///     A not type-safe version of memory barrier command.
    /// </summary>
    /// <param name="buffer">The buffer affected by the barrier.</param>
    /// <param name="barrierDesc">The barrier descriptor.</param>
    void MemoryBarrierUnsafe(BufferBase buffer, in MemoryBarrierDesc barrierDesc);

    /// <summary>
    ///     Insert a memory dependency.
    /// </summary>
    /// <param name="buffer">The buffer affected by the barrier.</param>
    /// <param name="sourceAccess">Source access mask.</param>
    /// <param name="destAccess">Destination access mask.</param>
    /// <typeparam name="T">Type of the elements stored in the buffer affected by the barrier.</typeparam>
    void MemoryBarrier<T>(Buffer<T> buffer, AccessFlags sourceAccess, AccessFlags destAccess)
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
    void Copy<T>(Buffer<T> source, Buffer<T> destination)
        where T : unmanaged
    {
        if (source.Descriptor.Size != destination.Descriptor.Size)
        {
            throw new InvalidOperationException("Source and destination sizes must be equal");
        }

        CopyUnsafe(source, destination, new BufferCopyRegion(source.Descriptor.Size));
    }

    /// <summary>
    ///     Copy a region of the source buffer to the destination buffer.
    /// </summary>
    /// <param name="source">Source buffer.</param>
    /// <param name="destination">Destination buffer.</param>
    /// <param name="region">Copy region.</param>
    /// <typeparam name="T">Type of the elements stored in the buffers.</typeparam>
    void Copy<T>(Buffer<T> source, Buffer<T> destination, in BufferCopyRegion region)
        where T : unmanaged
    {
        CopyUnsafe(source, destination, in region);
    }

    /// <summary>
    ///     A not type-safe version of buffer copy command.
    /// </summary>
    /// <param name="source">Source buffer.</param>
    /// <param name="destination">Destination buffer.</param>
    /// <param name="region">Copy region.</param>
    void CopyUnsafe(BufferBase source, BufferBase destination, in BufferCopyRegion region);

    /// <summary>
    ///     Dispatch a compute kernel to execute on the device.
    /// </summary>
    /// <param name="kernel">The kernel to dispatch.</param>
    /// <param name="x">The number of local workgroups to dispatch in the X dimension.</param>
    /// <param name="y">The number of local workgroups to dispatch in the Y dimension.</param>
    /// <param name="z">The number of local workgroups to dispatch in the Z dimension.</param>
    void Dispatch(Kernel kernel, int x, int y, int z);

    /// <summary>
    ///     Dispatch a compute kernel to execute on the device.
    /// </summary>
    /// <param name="kernel">The kernel to dispatch.</param>
    /// <param name="workgroups">The number of local workgroups to dispatch in the X, Y and Z dimensions.</param>
    void Dispatch(Kernel kernel, Vector3Int workgroups)
    {
        Dispatch(kernel, workgroups.X, workgroups.Y, workgroups.Z);
    }

    /// <summary>
    ///     Set the command list state to Executable and end command recording.
    /// </summary>
    void End();
}
