namespace UraniumCompute.Acceleration.Allocators;

public interface IDeviceAllocator
{
    public const ulong DefaultAlignment = 256;
    
    ulong AllocatedByteCount { get; }

    void Init(in Desc desc);
    NullableHandle Allocate(ulong byteSize, ulong byteAlignment = 0);
    void DeAllocate(NullableHandle ptr);
    void GarbageCollect();
    void Reset();
    
    public record struct Desc(NullableHandle AddressBase, ulong CapacityInBytes, ulong AlignmentInBytes, int GCLatency = 3)
    {
        public Desc(ulong capacityInBytes, int gcLatency = 3)
            : this(NullableHandle.Zero, capacityInBytes, DefaultAlignment, gcLatency)
        {
        }
    }
}
