using UraniumCompute.Acceleration.Pipelines;
using UraniumCompute.Backend;

namespace UraniumCompute.Acceleration.TransientResources;

public readonly struct TransientBuffer1D<T> : ITransientBuffer<Buffer1D<T>.Desc>
    where T : unmanaged
{
    public int Id { get; }
    public Buffer1D<T>.Desc Descriptor => pipeline.GetResourceDescriptor<Buffer1D<T>.Desc>(Id);
    public Buffer1D<T> Buffer => (Buffer1D<T>)pipeline.GetResource(Id);

    public BufferBase Resource => pipeline.GetResource(Id);

    /// <summary>
    ///     Size of a single buffer element in bytes.
    /// </summary>
    public static readonly int ElementSize = Buffer<T>.ElementSize;

    public ulong LongCount => Descriptor.XDimension;

    public int Count => (int)LongCount;

    public static readonly TransientBuffer1D<T> Null = new();

    private readonly Pipeline pipeline;

    internal TransientBuffer1D(Pipeline pipeline, int id)
    {
        this.pipeline = pipeline;
        Id = id;
    }
}
