using System.Runtime.InteropServices;

namespace UraniumCompute.Backend;

/// <summary>
///     Memory barrier descriptor.
/// </summary>
/// <param name="SourceAccess">Source access mask.</param>
/// <param name="DestAccess">Destination access mask.</param>
/// <param name="SourceQueueKind">Source command queue kind.</param>
/// <param name="DestQueueKind">Destination command queue kind.</param>
[StructLayout(LayoutKind.Sequential)]
public readonly record struct MemoryBarrierDesc(AccessFlags SourceAccess, AccessFlags DestAccess,
    HardwareQueueKindFlags SourceQueueKind = HardwareQueueKindFlags.None,
    HardwareQueueKindFlags DestQueueKind = HardwareQueueKindFlags.None);
