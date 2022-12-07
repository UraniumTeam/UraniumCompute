using UraniumCompute.Backend;

namespace UraniumCompute.Acceleration.TransientResources;

public interface ITransientBuffer<T>
    where T : unmanaged
{
    int Id { get; }
    BufferBase.Desc Descriptor { get; }
    
    /// <summary>
    ///     The number of elements in the buffer as <see cref="System.Int64" />.
    /// </summary>
    ulong LongCount { get; }

    /// <summary>
    ///     The number of elements in the buffer.
    /// </summary>
    int Count { get; }
}
