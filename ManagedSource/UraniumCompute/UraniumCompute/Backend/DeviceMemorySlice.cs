using System.Diagnostics;

namespace UraniumCompute.Backend;

/// <summary>
///     A slice of device memory.
/// </summary>
public readonly record struct DeviceMemorySlice
{
    /// <summary>
    ///     The underlying device memory object.
    /// </summary>
    public DeviceMemory Memory { get; init; }

    /// <summary>
    ///     The byte offset of the slice.
    /// </summary>
    public ulong Offset { get; init; }

    /// <summary>
    ///     The byte size of the slice.
    /// </summary>
    public ulong Size { get; init; }

    public DeviceMemorySlice(DeviceMemory memory, ulong offset = 0, ulong size = DeviceMemory.WholeSize)
    {
        Memory = memory;
        Offset = offset;
        Size = Math.Min(size, memory.Descriptor.Size - offset);
        Debug.Assert(Size == DeviceMemory.WholeSize || Size <= Memory.Descriptor.Size - Offset);
    }

    /// <summary>
    ///     Map the part of device memory represented by this slice.
    /// </summary>
    /// <param name="offset">Byte offset of the memory to map within this slice.</param>
    /// <param name="size">Size of the part of the memory to map.</param>
    /// <typeparam name="T">Type of elements stored in the memory.</typeparam>
    /// <returns>
    ///     A <see cref="MemoryMapHelper{T}" /> object that holds a pointer to the mapped memory.
    /// </returns>
    public unsafe MemoryMapHelper<T> Map<T>(ulong offset = 0, ulong size = DeviceMemory.WholeSize)
        where T : unmanaged
    {
        var ptr = (T*)Memory.MapImpl(Offset + offset, Math.Min(size, Size - offset));
        return new MemoryMapHelper<T>(in this, ptr);
    }

    /// <inheritdoc cref="DeviceMemory.Unmap" />
    public void Unmap()
    {
        Memory.Unmap();
    }
}
