namespace UraniumCompute.Backend;

/// <summary>
///     Command list allocation flags.
/// </summary>
[Flags]
public enum CommandListFlags
{
    None = 0,

    /// The command list will be invalid after the first call to submit.
    OneTimeSubmit = 1 << 0
}
