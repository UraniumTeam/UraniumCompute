using UraniumCompute.Backend;

namespace UraniumCompute.Acceleration.TransientResources;

public interface ITransientBuffer<out TDesc> : ITransientResource
{
    TDesc Descriptor { get; }
    
    /// <summary>
    ///     The number of elements in the buffer as <see cref="System.Int64" />.
    /// </summary>
    ulong LongCount { get; }

    /// <summary>
    ///     The number of elements in the buffer.
    /// </summary>
    int Count { get; }
}
