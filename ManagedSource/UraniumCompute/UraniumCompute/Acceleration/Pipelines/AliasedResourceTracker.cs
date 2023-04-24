using UraniumCompute.Acceleration.TransientResources;
using UraniumCompute.Backend;

namespace UraniumCompute.Acceleration.Pipelines;

internal sealed class AliasedResourceTracker
{
    private readonly List<TransientResourceInfo> resources = new();
    private readonly HashSet<(TransientResourceInfo, TransientResourceInfo)> barriers = new();

    public void Reset()
    {
        resources.Clear();
        barriers.Clear();
    }

    public void Add(in TransientResourceInfo newResource)
    {
        for (var i = 0; i < resources.Count; ++i)
        {
            var oldResource = resources[i];
            var intersection = oldResource.Intersect(newResource);

            switch (intersection)
            {
                case TransientResourceInfo.Intersection.None:
                    // Resources do not overlap, so there's no need to synchronize access to them
                    break;
                case TransientResourceInfo.Intersection.Full:
                    AddBarrierIfNeeded(in oldResource, in newResource);
                    resources.RemoveAt(i--);
                    break;
                case TransientResourceInfo.Intersection.Partial:
                    AddBarrierIfNeeded(in oldResource, in newResource);

                    var recycledOld = false;
                    if (oldResource.MinOffset < newResource.MinOffset)
                    {
                        recycledOld = true;
                        resources[i] = resources[i] with { MaxOffset = newResource.MinOffset - 1 };
                    }

                    if (oldResource.MaxOffset > newResource.MaxOffset)
                    {
                        if (!recycledOld)
                        {
                            resources[i] = resources[i] with { MinOffset = newResource.MaxOffset + 1 };
                        }
                        else
                        {
                            var right = oldResource;
                            right.MinOffset = newResource.MaxOffset + 1;
                            resources.Insert(i + 1, right);
                            ++i;
                        }
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        var wasInserted = false;
        for (var i = 0; i < resources.Count; ++i)
        {
            if (resources[i].MinOffset > newResource.MinOffset)
            {
                resources.Insert(i, newResource);
                wasInserted = true;
                break;
            }
        }

        if (!wasInserted)
        {
            resources.Add(newResource);
        }
    }

    private void AddBarrierIfNeeded(in TransientResourceInfo oldResource, in TransientResourceInfo newResource)
    {
        if (barriers.Contains((oldResource, newResource)))
        {
            return;
        }

        Console.WriteLine($"Buffer alias: {oldResource.Resource} -> {newResource.Resource}");

        barriers.Add((oldResource, newResource));

        var barrier = new MemoryBarrierDesc(AccessFlags.All, AccessFlags.All);
        newResource.Creator.AddBarrier(barrier, oldResource.Resource);
    }
}
