using UraniumCompute.Acceleration.Pipelines;
using UraniumCompute.Backend;

namespace UraniumCompute.Acceleration.TransientResources;

public readonly struct TransientBuffer2D<T> : ITransientBuffer<Buffer2D<T>.Desc>
    where T : unmanaged
{
    public int Id { get; }
    public Buffer2D<T>.Desc Descriptor => pipeline.GetResourceDescriptor<Buffer2D<T>.Desc>(Id);
    public Buffer2D<T> Buffer => (Buffer2D<T>)pipeline.GetResource(Id);

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

    public static readonly TransientBuffer2D<T> Null = new();

    private readonly Pipeline pipeline;

    internal TransientBuffer2D(Pipeline pipeline, int id)
    {
        this.pipeline = pipeline;
        Id = id;
    }
}
