namespace UraniumCompute.Backend;

/// <summary>
///     State of a command list.
/// </summary>
public enum CommandListState
{
    /// <summary>
    ///     The state of command list when it was just initialized or reset.
    /// </summary>
    Initial,

    /// <summary>
    ///     The state of command list when the <see cref="CommandList.Begin" /> was called.
    /// </summary>
    Recording,

    /// <summary>
    ///     The state of command list when the recording ended by calling <see cref="CommandList.Builder.Dispose" />.
    /// </summary>
    Executable,

    /// <summary>
    ///     The state of command list when it was submitted and execution of the commands is still in progress.
    /// </summary>
    Pending,

    /// <summary>
    ///     The state of command list when it is uninitialized yet or completed execution while having
    ///     <see cref="CommandListFlags.OneTimeSubmit" /> flag.
    /// </summary>
    Invalid
}
