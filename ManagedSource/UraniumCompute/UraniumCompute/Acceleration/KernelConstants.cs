using UraniumCompute.Backend;
using UraniumCompute.Memory;

namespace UraniumCompute.Acceleration;

/// <summary>
///     Encapsulates backend-specific buffers that store the kernel constant data on the device.
/// </summary>
public sealed class KernelConstants<T> : BufferBase
    where T : unmanaged
{
    /// <summary>
    ///     Size of the constants in bytes.
    /// </summary>
    public int Size => ElementSize;

    private static readonly unsafe int ElementSize = sizeof(T);

    internal KernelConstants(nint handle) : base(handle)
    {
    }

    /// <summary>
    ///     Map the memory bound to this buffer.
    /// </summary>
    /// <returns>Memory mapping helper object that maps to buffer memory.</returns>
    public unsafe StructMapper<T> Map()
    {
        var ptr = (T*)BoundMemory.Memory.MapImpl();
        return new StructMapper<T>(BoundMemory, ptr);
    }

    /// <summary>
    ///     Set value of a kernel constant.
    /// </summary>
    /// <param name="value">The value to set.</param>
    public void Set(in T value)
    {
        using var map = Map();
        map.Value = value;
    }

    /// <summary>
    ///     Get value of a kernel constant.
    /// </summary>
    public T Get()
    {
        using var map = Map();
        return map.Value;
    }

    /// <summary>
    ///     Initialize the device object.
    /// </summary>
    /// <param name="name">Debug name of the object.</param>
    public void Init(NativeString name)
    {
        base.Init(new Desc(name, (ulong)ElementSize, Usage.Constant));
    }
}
