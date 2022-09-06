using System.Diagnostics.CodeAnalysis;
using UraniumCompute.Utils;

namespace UraniumCompute.Backend;

/// <summary>
///     A slice of device memory.
/// </summary>
public readonly record struct DeviceMemorySlice : IValidatable
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
    }

    public bool HasValidationErrors([MaybeNullWhen(false)] out string errorMessage)
    {
        if (Size == DeviceMemory.WholeSize || Size <= Memory.Descriptor.Size - Offset)
        {
            errorMessage = null;
            return false;
        }

        errorMessage = $"Size = {Size}, but memory has only {Memory.Descriptor.Size} - {Offset} bytes left";
        return true;
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
