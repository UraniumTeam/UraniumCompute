using UraniumCompute.Acceleration.Pipelines;
using UraniumCompute.Acceleration.TransientResources;

namespace UraniumCompute.Acceleration.Jobs;

public sealed class DeviceCopyJob<T> : IDeviceJob
    where T : unmanaged
{
    public string Name { get; }

    public TransientBuffer1D<T> Source { get; }
    public TransientBuffer1D<T> Destination => destination;

    private TransientBuffer1D<T> destination;

    private readonly MemoryKindFlags destinationMemoryKindFlags;

    internal DeviceCopyJob(string name, TransientBuffer1D<T> source, TransientBuffer1D<T> destination)
    {
        Name = name;
        Source = source;
        this.destination = destination;
    }

    internal DeviceCopyJob(string name, TransientBuffer1D<T> source, MemoryKindFlags destinationMemoryKindFlags)
    {
        Name = name;
        Source = source;
        destination = null!;
        this.destinationMemoryKindFlags = destinationMemoryKindFlags;
    }

    public void Run(IJobRunContext ctx)
    {
    }

    public IJobSetupContext Setup(IDeviceJobSetupContext ctx)
    {
        if (destinationMemoryKindFlags == MemoryKindFlags.None)
        {
            return ctx
                .Read(Source)
                .Write(Destination);
        }

        return ctx
            .Read(Source)
            .CreateBuffer(out destination, "Copy destination", Source.LongCount, destinationMemoryKindFlags);
    }
}
