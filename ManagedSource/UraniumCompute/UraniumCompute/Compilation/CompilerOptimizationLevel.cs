namespace UraniumCompute.Compilation;

/// <summary>
///     Flags used to control compiler optimization level.
/// </summary>
public enum CompilerOptimizationLevel
{
    /// <summary>
    ///     No optimization.
    /// </summary>
    None,

    /// <summary>
    ///     Level 1 optimization.
    /// </summary>
    O1,

    /// <summary>
    ///     Level 2 optimization.
    /// </summary>
    O2,

    /// <summary>
    ///     Level 3 optimization.
    /// </summary>
    O3,

    /// <summary>
    ///     Maximum level of optimization supported by the compiler.
    /// </summary>
    Max = O3
}
