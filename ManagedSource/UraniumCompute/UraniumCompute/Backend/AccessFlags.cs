namespace UraniumCompute.Backend;

/// <summary>
///     Resource access flags used for memory barriers.
/// </summary>
[Flags]
public enum AccessFlags
{
    None = 1 << 0,
    KernelRead = 1 << 1,
    KernelWrite = 1 << 2,
    TransferRead = 1 << 3,
    TransferWrite = 1 << 4,
    HostRead = 1 << 5,
    HostWrite = 1 << 6,

    All = KernelRead | KernelWrite |
          TransferRead | TransferWrite |
          HostRead | HostWrite
}
