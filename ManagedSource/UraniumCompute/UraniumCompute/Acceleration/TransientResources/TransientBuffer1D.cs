using UraniumCompute.Acceleration.Pipelines;
using UraniumCompute.Backend;

namespace UraniumCompute.Acceleration.TransientResources;

public readonly struct TransientBuffer1D<T> : ITransientBuffer<T>
    where T : unmanaged
{
    public int Id { get; }
    public BufferBase.Desc Descriptor => pipeline.GetResourceDescriptor(Id);
    public Buffer1D<T> Buffer => (Buffer1D<T>)pipeline.GetResource(Id);

    /// <summary>
    ///     Size of a single buffer element in bytes.
    /// </summary>
    public static readonly int ElementSize = Buffer<T>.ElementSize;

    public ulong LongCount => Descriptor.Size / (ulong)ElementSize;

    public int Count => (int)LongCount;

    public static readonly TransientBuffer1D<T> Null = new();

    private readonly Pipeline pipeline;

    internal TransientBuffer1D(Pipeline pipeline, int id)
    {
        this.pipeline = pipeline;
        Id = id;
    }
}
