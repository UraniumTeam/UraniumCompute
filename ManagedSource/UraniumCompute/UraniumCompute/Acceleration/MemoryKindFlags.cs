namespace UraniumCompute.Acceleration;

/// <summary>
///     Device memory kind flags.
/// </summary>
[Flags]
public enum MemoryKindFlags
{
    /// <summary>
    ///     Invalid or unspecified value.
    /// </summary>
    None = 0,

    /// <summary>
    ///     Host (CPU) accessible memory.
    /// </summary>
    HostAccessible = 1 << 0,

    /// <summary>
    ///     Device (GPU for accelerated backends) accessible memory.
    /// </summary>
    DeviceAccessible = 1 << 1,

    /// <summary>
    ///     Memory accessible for both the device and the host.
    /// </summary>
    HostAndDeviceAccessible = HostAccessible | DeviceAccessible
}
