namespace UraniumCompute.Backend;

/// <summary>
///     Flags that store the kinds of operations that can be executed using a GPU hardware queue.
/// </summary>
[Flags]
public enum HardwareQueueKindFlags
{
    /// Invalid or unspecified value.
    None = 0,

    /// Queue that supports graphics operations.
    GraphicsBit = 1 << 0,

    /// Queue that supports compute operations.
    ComputeBit = 1 << 1,

    /// Queue that supports copy operations.
    TransferBit = 1 << 2,

    /// Queue for graphics + compute + copy operations.
    Graphics = GraphicsBit | ComputeBit | TransferBit,

    /// Queue for compute + copy operations.
    Compute = ComputeBit | TransferBit,

    /// Queue for copy operations.
    Transfer = TransferBit
}
