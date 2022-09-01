using System.Diagnostics.CodeAnalysis;
using UraniumCompute.Utils;

namespace UraniumCompute.Backend;

public readonly record struct DeviceMemorySlice : IValidatable
{
    public DeviceMemory Memory { get; init; }
    public ulong Offset { get; init; }
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

    public unsafe MemoryMapHelper<T> Map<T>(ulong offset = 0, ulong size = DeviceMemory.WholeSize)
        where T : unmanaged
    {
        var ptr = (T*)Memory.MapImpl(Offset + offset, Math.Min(size, Size - offset));
        return new MemoryMapHelper<T>(in this, ptr);
    }

    public void Unmap()
    {
        Memory.Unmap();
    }
}
