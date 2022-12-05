namespace UraniumCompute.Acceleration.Allocators;

public interface IDeviceAllocator
{
    public const ulong DefaultAlignment = 256;
    
    ulong AllocatedByteCount { get; }
    
    NullableHandle Allocate(ulong byteSize, ulong bytAlignment = 0);
    void DeAllocate(NullableHandle ptr);
    void GarbageCollect();
    void Reset();
}
