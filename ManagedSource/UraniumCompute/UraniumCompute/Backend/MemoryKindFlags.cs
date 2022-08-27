namespace UraniumCompute.Backend;

/// <summary>
/// Device memory kind flags.
/// </summary>
public enum MemoryKindFlags
{
    /// Invalid or unspecified value.
    None = 0,

    /// Host (CPU) accessible memory.
    HostAccessible = 1 << 0,

    /// Device (GPU for accelerated backends) accessible memory.
    DeviceAccessible = 1 << 1,

    /// Memory accessible for both the device and the host.
    HostAndDeviceAccessible = HostAccessible | DeviceAccessible
}
