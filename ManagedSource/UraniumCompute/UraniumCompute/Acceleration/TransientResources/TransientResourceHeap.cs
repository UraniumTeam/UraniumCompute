﻿using UraniumCompute.Acceleration.Allocators;
using UraniumCompute.Acceleration.Pipelines;
using UraniumCompute.Backend;
using UraniumCompute.Memory;
using UraniumCompute.Utils;

namespace UraniumCompute.Acceleration.TransientResources;

public sealed class TransientResourceHeap : IDisposable
{
    public IDeviceAllocator Allocator => Descriptor.Allocator;
    public DeviceMemory Memory { get; }
    public Desc Descriptor { get; private set; }
    public bool IsInitialized { get; private set; }

    private readonly ComputeDevice device;
    private readonly AliasedResourceTracker resourceTracker = new();
    private Cache<int, BufferBase> cache;
    private readonly Dictionary<int, TransientResourceInfo> registeredResources = new();

    private readonly Buffer1D<int> referenceBuffer;

    internal TransientResourceHeap(ComputeDevice device)
    {
        this.device = device;
        referenceBuffer = device.CreateBuffer1D<int>();
        Memory = device.CreateMemory();
        cache = new Cache<int, BufferBase>(1, CacheReplacementPolicy.ThrowException);
    }

    public void Init(MemoryKindFlags memoryKindFlags, int gcLatency, ulong heapSize = 256 * 1024)
    {
        Init(new Desc(heapSize, 256, 256, gcLatency, memoryKindFlags, new FreeListDeviceAllocator()));
    }

    public void Init(in Desc desc)
    {
        if (IsInitialized)
        {
            throw new InvalidOperationException($"{nameof(TransientResourceHeap)} was already initialized");
        }

        IsInitialized = true;
        Descriptor = desc;
        referenceBuffer.Init($"{nameof(TransientResourceHeap)} reference buffer", 1);

        cache = new Cache<int, BufferBase>(desc.CacheSize);

        ReadOnlySpan<nint> bufferHandle = stackalloc nint[] { referenceBuffer.Handle };
        Memory.Init(new DeviceMemory.Desc($"{nameof(TransientResourceHeap)} memory", Descriptor.ByteSize, bufferHandle,
            desc.MemoryKindFlags));

        Allocator.Init(new IDeviceAllocator.Desc(NullableHandle.Zero, desc.ByteSize, desc.ByteAlignment,
            desc.GCLatency));
    }

    public Buffer1D<T> CreateBuffer1D<T>(int id, Buffer1D<T>.Desc desc, IJobContext creator)
        where T : unmanaged
    {
        if (!IsInitialized)
        {
            throw new InvalidOperationException($"{nameof(TransientResourceHeap)} was uninitialized");
        }

        var baseDesc = Buffer1D<T>.CreateDesc(desc);
        var address = Allocator.Allocate(baseDesc.Size, Descriptor.ByteAlignment);
        if (address.IsNull)
        {
            throw new OutOfMemoryException();
        }

        Buffer1D<T> result;
        var descHash = HashCode.Combine(baseDesc, address);
        if (cache.TryGetValue(descHash, out var cached))
        {
            result = (Buffer1D<T>)cached;
        }
        else
        {
            var newBuffer = device.CreateBuffer1D<T>();
            newBuffer.Init(baseDesc);
            result = newBuffer;
            cache[descHash] = result;

            var memory = new DeviceMemorySlice(Memory, (ulong)address, baseDesc.Size);
            result.BindMemory(memory);
        }

        registeredResources[id] =
            new TransientResourceInfo(result, creator, address, baseDesc.Size);
        return result;
    }

    public Buffer2D<T> CreateBuffer2D<T>(int id, Buffer2D<T>.Desc desc, IJobContext creator)
        where T : unmanaged
    {
        if (!IsInitialized)
        {
            throw new InvalidOperationException($"{nameof(TransientResourceHeap)} was uninitialized");
        }

        var baseDesc = Buffer2D<T>.CreateDesc(desc);
        var address = Allocator.Allocate(baseDesc.Size, Descriptor.ByteAlignment);
        if (address.IsNull)
        {
            throw new OutOfMemoryException();
        }

        Buffer2D<T> result;
        var descHash = HashCode.Combine(baseDesc, address);
        if (cache.TryGetValue(descHash, out var cached))
        {
            result = (Buffer2D<T>)cached;
        }
        else
        {
            var newBuffer = device.CreateBuffer2D<T>();
            newBuffer.Init(baseDesc);
            result = newBuffer;
            cache[descHash] = result;

            var memory = new DeviceMemorySlice(Memory, (ulong)address, baseDesc.Size);
            result.BindMemory(memory);
        }

        registeredResources[id] =
            new TransientResourceInfo(result, creator, address, baseDesc.Size);
        return result;
    }

    public void ReleaseResource(int id)
    {
        if (!IsInitialized)
        {
            throw new InvalidOperationException($"{nameof(TransientResourceHeap)} was uninitialized");
        }

        var resource = registeredResources[id];
        Allocator.DeAllocate(resource.Address);
        resourceTracker.Add(in resource);
    }

    public record struct Desc(ulong ByteSize, ulong ByteAlignment, int CacheSize, int GCLatency, MemoryKindFlags MemoryKindFlags,
        IDeviceAllocator Allocator);

    public void Dispose()
    {
        foreach (var (_, resource) in cache)
        {
            resource.Dispose();
        }

        Memory.Dispose();
        referenceBuffer.Dispose();
    }
}
