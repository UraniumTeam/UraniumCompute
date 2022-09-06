namespace UraniumCompute.Backend;

/// <summary>
///     Flags that store the kinds of operations that can be executed using a GPU hardware queue.
/// </summary>
[Flags]
public enum HardwareQueueKindFlags
{
    /// <summary>
    ///     Invalid or unspecified value.
    /// </summary>
    None = 0,

    /// <summary>
    ///     Queue that supports graphics operations.
    /// </summary>
    GraphicsBit = 1 << 0,

    /// <summary>
    ///     Queue that supports compute operations.
    /// </summary>
    ComputeBit = 1 << 1,

    /// <summary>
    ///     Queue that supports copy operations.
    /// </summary>
    TransferBit = 1 << 2,

    /// <summary>
    ///     Queue for graphics + compute + copy operations.
    /// </summary>
    Graphics = GraphicsBit | ComputeBit | TransferBit,

    /// <summary>
    ///     Queue for compute + copy operations.
    /// </summary>
    Compute = ComputeBit | TransferBit,

    /// <summary>
    ///     Queue for copy operations.
    /// </summary>
    Transfer = TransferBit
}
