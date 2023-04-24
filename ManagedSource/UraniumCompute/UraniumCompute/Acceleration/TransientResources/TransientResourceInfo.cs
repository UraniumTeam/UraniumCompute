using UraniumCompute.Acceleration.Allocators;
using UraniumCompute.Acceleration.Pipelines;
using UraniumCompute.Backend;

namespace UraniumCompute.Acceleration.TransientResources;

internal struct TransientResourceInfo
{
    public readonly NullableHandle Address;
    public readonly BufferBase Resource;
    public readonly IJobContext Creator;

    public ulong MinOffset;
    public ulong MaxOffset;

    public TransientResourceInfo(BufferBase resource, IJobContext creator, NullableHandle address, ulong allocationSize)
    {
        Address = address;
        Resource = resource;
        Creator = creator;
        MinOffset = (ulong)Address;
        MaxOffset = MinOffset + allocationSize - 1;
    }

    internal Intersection Intersect(in TransientResourceInfo newResource)
    {
        if (MaxOffset < newResource.MinOffset || MinOffset > newResource.MaxOffset)
        {
            return Intersection.None;
        }

        if (MinOffset >= newResource.MinOffset && MaxOffset <= newResource.MaxOffset)
        {
            return Intersection.Full;
        }

        return Intersection.Partial;
    }

    internal enum Intersection
    {
        None,
        Full,
        Partial
    }
}
