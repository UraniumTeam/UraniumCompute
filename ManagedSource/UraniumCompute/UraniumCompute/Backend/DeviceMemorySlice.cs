namespace UraniumCompute.Backend;

public record struct DeviceMemorySlice(DeviceMemory Memory, long Offset = 0, long Size = long.MaxValue);
