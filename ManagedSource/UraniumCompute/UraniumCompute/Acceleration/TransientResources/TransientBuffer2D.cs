using UraniumCompute.Acceleration.Pipelines;
using UraniumCompute.Backend;

namespace UraniumCompute.Acceleration.TransientResources;

public sealed class TransientBuffer2D<T> : ITransientBuffer<Buffer2D<T>.Desc>
    where T : unmanaged
{
    public int Id { get; set; }
    public Buffer2D<T>.Desc Descriptor { get; }
    public Buffer2D<T> Buffer => (Buffer2D<T>)Resource;

    public BufferBase Resource { get; set; } = null!;

    public ICollection<IComputeJob> Readers { get; } = new List<IComputeJob>();
    public ICollection<IComputeJob> Writers { get; } = new List<IComputeJob>();

    public IComputeJob Creator { get; set; } = null!;
    public IComputeJob Deleter { get; set; } = null!;

    public MemoryKindFlags MemoryKindFlags { get; }
    public AccessFlags CurrentAccess { get; set; }

    /// <summary>
    ///     Size of a single buffer element in bytes.
    /// </summary>
    public static readonly int ElementSize = Buffer<T>.ElementSize;

    public ulong LongCount => Descriptor.XDimension * Descriptor.YDimension;

    public int Count => (int)LongCount;

    /// <summary>
    ///     The width of the buffer.
    /// </summary>
    public int Width => (int)LongWidth;

    /// <summary>
    ///     The height of the buffer.
    /// </summary>
    public int Height => (int)LongHeight;

    /// <summary>
    ///     The width of the buffer as <see cref="System.Int64" />.
    /// </summary>
    public ulong LongWidth => Descriptor.XDimension;

    /// <summary>
    ///     The height of the buffer as <see cref="System.Int64" />.
    /// </summary>
    public ulong LongHeight => Descriptor.YDimension;

    /// <summary>
    ///     Can be used to suppress nullable reference type warnings.
    /// </summary>
    public static readonly TransientBuffer2D<T> Null = null!;

    internal TransientBuffer2D(Buffer2D<T>.Desc desc, MemoryKindFlags memoryKindFlags)
    {
        Descriptor = desc;
        MemoryKindFlags = memoryKindFlags;
    }
}
