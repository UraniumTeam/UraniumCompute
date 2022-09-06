namespace UraniumCompute.Backend;

/// <summary>
///     State of fence (signaled or reset).
/// </summary>
public enum FenceState
{
    /// <summary>
    ///     The fence was signaled.
    /// </summary>
    Signaled,

    /// <summary>
    ///     The fence was reset or wasn't signaled.
    /// </summary>
    Reset
}
