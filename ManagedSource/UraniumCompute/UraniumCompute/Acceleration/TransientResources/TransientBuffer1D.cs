using UraniumCompute.Acceleration.Pipelines;
using UraniumCompute.Backend;

namespace UraniumCompute.Acceleration.TransientResources;

public sealed class TransientBuffer1D<T> : ITransientBuffer<Buffer1D<T>.Desc>
    where T : unmanaged
{
    public int Id { get; set; }
    public Buffer1D<T>.Desc Descriptor { get; }
    public Buffer1D<T> Buffer => (Buffer1D<T>)Resource;

    public BufferBase Resource { get; set; } = null!;

    public ICollection<IComputeJob> Readers { get; } = new List<IComputeJob>();
    public ICollection<IComputeJob> Writers { get; } = new List<IComputeJob>();
    
    public IComputeJob Creator { get; set; } = null!;
    public IComputeJob Deleter { get; set; } = null!;

    public MemoryKindFlags MemoryKindFlags { get; }

    /// <summary>
    ///     Size of a single buffer element in bytes.
    /// </summary>
    public static readonly int ElementSize = Buffer<T>.ElementSize;

    public ulong LongCount => Descriptor.XDimension;

    public int Count => (int)LongCount;

    /// <summary>
    ///     Can be used to suppress nullable reference type warnings.
    /// </summary>
    public static readonly TransientBuffer1D<T> Null = null!;

    internal TransientBuffer1D(Buffer1D<T>.Desc desc, MemoryKindFlags memoryKindFlags)
    {
        Descriptor = desc;
        MemoryKindFlags = memoryKindFlags;
    }
}
