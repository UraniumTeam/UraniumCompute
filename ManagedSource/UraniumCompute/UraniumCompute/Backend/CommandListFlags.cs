namespace UraniumCompute.Backend;

/// <summary>
///     Command list allocation flags.
/// </summary>
[Flags]
public enum CommandListFlags
{
    None = 0,

    /// <summary>
    ///     The command list will be invalid after the first call to submit.
    /// </summary>
    OneTimeSubmit = 1 << 0
}
