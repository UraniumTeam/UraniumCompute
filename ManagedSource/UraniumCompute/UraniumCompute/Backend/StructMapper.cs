namespace UraniumCompute.Backend;

/// <summary>
///     Helper class for memory mapping, specific for single values (e.g. constant buffers).
/// </summary>
/// <typeparam name="T">Type of the element stored in the mapped memory.</typeparam>
public sealed class StructMapper<T> : MemoryMapper<T>
    where T : unmanaged
{
    /// <summary>
    ///     A reference to the value stored in the mapped memory.
    /// </summary>
    public ref T Value => ref GetElementAt(0);

    internal unsafe StructMapper(in DeviceMemorySlice slice, T* map) : base(slice, map)
    {
    }
}
