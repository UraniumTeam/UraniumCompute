using System.Diagnostics;
using Desc = UraniumCompute.Acceleration.Allocators.IDeviceAllocator.Desc;

namespace UraniumCompute.Acceleration.Allocators;

public sealed class FreeListDeviceAllocator : IDeviceAllocator
{
    public ulong AllocatedByteCount { get; private set; }
    public Desc Descriptor { get; private set; }

    private int headNode;
    private readonly Policy policy;
    private readonly Stack<int> nodeFreeList = new();
    private readonly List<Node> nodes = new();
    private readonly Dictionary<ulong, AllocationInfo> allocations = new();
    private readonly Queue<Garbage> garbage = new();
    private int gcCycle;

    public enum Policy
    {
        FirstFit,
        BestFit
    }

    public FreeListDeviceAllocator(Policy policy = Policy.BestFit)
    {
        this.policy = policy;
    }

    public void Init(in Desc desc)
    {
        if (desc.CapacityInBytes == 0)
        {
            throw new ArgumentException("Capacity must be greater than zero");
        }

        Descriptor = desc;
        Reset();
        headNode = CreateNode();
        InsertNode(headNode, 0, desc.CapacityInBytes);
    }

    public void Reset()
    {
        garbage.Clear();
        gcCycle = 0;
        AllocatedByteCount = 0;
        nodeFreeList.Clear();
        nodes.Clear();
        allocations.Clear();
    }

    public NullableHandle Allocate(ulong byteSize, ulong byteAlignment = 0)
    {
        if (byteSize == 0)
        {
            return NullableHandle.Null;
        }

        byteAlignment = Math.Max(byteAlignment, Descriptor.AlignmentInBytes);
        var foundNode = FindNode(byteSize, byteAlignment);
        if (!foundNode.IsValid)
        {
            return NullableHandle.Null;
        }

        var foundIdx = foundNode.FoundIndex;
        var sizeAligned = nodes[foundIdx].Size - foundNode.LeftoverSize;
        var paddingByteSize = sizeAligned - byteSize;
        var address = nodes[foundIdx].AddressOffset + paddingByteSize;

        allocations[address] = new AllocationInfo(byteSize, paddingByteSize);

        if (foundNode.LeftoverSize > 0)
        {
            InsertNode(foundNode.FoundIndex, nodes[foundIdx].AddressOffset + sizeAligned, foundNode.LeftoverSize);
        }

        RemoveNode(foundNode.PreviousIndex, foundIdx);

        AllocatedByteCount += sizeAligned;
        if (AllocatedByteCount > Descriptor.CapacityInBytes)
        {
            throw new OutOfMemoryException();
        }

        return Descriptor.AddressBase + address;
    }

    public void DeAllocate(NullableHandle ptr)
    {
        if (ptr.IsNull)
        {
            return;
        }

        var addressOffset = (ulong)(ptr - Descriptor.AddressBase);
        if (addressOffset >= Descriptor.CapacityInBytes)
        {
            throw new ArgumentException($"The {ptr} was not allocated using this allocator", nameof(ptr));
        }

        garbage.Enqueue(new Garbage(addressOffset, gcCycle));
    }

    public void GarbageCollect()
    {
        while (garbage.Any() && IsGarbageReady(garbage.Peek()))
        {
            GarbageCollectInternal(garbage.Dequeue());
        }

        gcCycle++;
    }

    private bool IsGarbageReady(in Garbage g)
    {
        return gcCycle - g.GCCycle >= Descriptor.GCLatency;
    }

    private int CreateNode()
    {
        if (nodeFreeList.Any())
        {
            return nodeFreeList.Pop();
        }

        nodes.Add(new Node());
        return nodes.Count - 1;
    }

    private int InsertNode(int previousNode, ulong address, ulong size)
    {
        var current = CreateNode();
        nodes[current] = new Node(nodes[previousNode].NextFree, address, size);
        nodes[previousNode] = nodes[previousNode] with { NextFree = current };
        return current;
    }

    private void RemoveNode(int previousNode, int currentNode)
    {
        nodes[previousNode] = nodes[previousNode] with { NextFree = nodes[currentNode].NextFree };
        nodes[currentNode] = nodes[currentNode] with { NextFree = -1 };
        ReleaseNode(currentNode);
    }

    private void ReleaseNode(int node)
    {
        nodeFreeList.Push(node);
    }

    private ulong GetRequiredSize(ulong requestedSize, ulong requestedAlignment, ulong addressOffset)
    {
        var addressFull = Descriptor.AddressBase + addressOffset;
        var alignedAddress = addressFull.AlignUp(requestedAlignment);
        var alignmentPad = (ulong)(alignedAddress - addressFull);
        return requestedSize + alignmentPad;
    }

    private FoundNode FindNode(ulong size, ulong alignment)
    {
        return policy switch
        {
            Policy.FirstFit => FindNodeFirst(size, alignment),
            Policy.BestFit => FindNodeBest(size, alignment),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private FoundNode FindNodeFirst(ulong size, ulong alignment)
    {
        var previous = headNode;
        var nodeIndex = nodes[headNode].NextFree;

        while (nodeIndex >= 0)
        {
            var requiredSize = GetRequiredSize(size, alignment, nodes[nodeIndex].AddressOffset);

            if (requiredSize <= nodes[nodeIndex].Size)
            {
                return new FoundNode(nodes[nodeIndex].Size - requiredSize, previous, nodeIndex);
            }

            previous = nodeIndex;
            nodeIndex = nodes[nodeIndex].NextFree;
        }

        return FoundNode.Invalid;
    }

    private FoundNode FindNodeBest(ulong size, ulong alignment)
    {
        var leftoverSizeMin = ulong.MaxValue;

        var result = FoundNode.Invalid;
        var previous = headNode;
        var nodeIndex = nodes[headNode].NextFree;
        while (nodeIndex >= 0)
        {
            var requiredSize = GetRequiredSize(size, alignment, nodes[nodeIndex].AddressOffset);

            if (requiredSize <= nodes[nodeIndex].Size)
            {
                var leftoverSize = nodes[nodeIndex].Size - requiredSize;
                if (leftoverSize < leftoverSizeMin)
                {
                    result = new FoundNode(leftoverSize, previous, nodeIndex);
                    leftoverSizeMin = leftoverSize;
                }
            }

            previous = nodeIndex;
            nodeIndex = nodes[nodeIndex].NextFree;
        }

        return result;
    }

    private void GarbageCollectInternal(Garbage g)
    {
        if (!allocations.TryGetValue(g.Address, out var allocation))
        {
            return;
        }

        FreeInternal(g.Address, allocation);

        var sizeAligned = allocation.ByteSize + allocation.PaddingByteSize;
        if (AllocatedByteCount < sizeAligned)
        {
            throw new ArgumentException("Allocator underflow");
        }

        AllocatedByteCount -= sizeAligned;
        allocations.Remove(g.Address);
    }

    private void FreeInternal(ulong address, AllocationInfo allocation)
    {
        var blockAddress = address - allocation.PaddingByteSize;
        var blockSize = allocation.ByteSize + allocation.PaddingByteSize;

        var previous = headNode;
        var next = -1;
        var current = nodes[headNode].NextFree;
        while (current >= 0)
        {
            var node = nodes[current];
            if (blockAddress < node.AddressOffset)
            {
                next = current;
                break;
            }

            previous = current;
            current = node.NextFree;
        }

        var previousMerged = false;

        if (previous != headNode && nodes[previous].End == blockAddress)
        {
            nodes[previous] = nodes[previous].AddSize(blockSize);
            previousMerged = true;
        }

        if (!previousMerged)
        {
            previous = InsertNode(previous, blockAddress, blockSize);
        }

        if (next >= 0)
        {
            var nodePrevious = nodes[previous];
            var nodeNext = nodes[next];
            Debug.Assert(nodePrevious.AddressOffset < nodeNext.AddressOffset, "Allocations not sorted");
            if (nodePrevious.End == nodeNext.AddressOffset)
            {
                nodes[previous] = nodePrevious.AddSize(nodeNext.Size);
                RemoveNode(previous, next);
            }
        }
    }

    private readonly record struct Garbage(ulong Address, int GCCycle);

    private readonly record struct Node(int NextFree = -1, ulong AddressOffset = 0, ulong Size = 0)
    {
        public ulong End => AddressOffset + Size;

        public Node AddSize(ulong byteCount)
        {
            return this with { Size = Size + byteCount };
        }
    }

    private readonly record struct FoundNode(ulong LeftoverSize, int PreviousIndex, int FoundIndex)
    {
        public static readonly FoundNode Invalid = new(0, -1, -1);

        public bool IsValid => FoundIndex != -1;
    }

    private readonly record struct AllocationInfo(ulong ByteSize, ulong PaddingByteSize);
}
