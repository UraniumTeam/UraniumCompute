namespace UraniumCompute.Backend;

/// <summary>
///     State of a command list.
/// </summary>
public enum CommandListState
{
    Initial,
    Recording,
    Executable,
    Pending,
    Invalid
}
