namespace UraniumCompute.Acceleration;

/// <summary>
///     Represents a general result of a function call within the library.
///     Different functions may have their own result codes, but this enum should be enough for a general case.
/// </summary>
public enum ResultCode : uint
{
    /// <summary>
    ///     Operation succeeded.
    /// </summary>
    Success,

    /// <summary>
    ///     Operation failed.
    /// </summary>
    Fail,

    /// <summary>
    ///     Operation aborted.
    /// </summary>
    Abort,

    /// <summary>
    ///     Operation not implemented.
    /// </summary>
    NotImplemented,

    /// <summary>
    ///     Operation was invalid.
    /// </summary>
    InvalidOperation,

    /// <summary>
    ///     One or more arguments were invalid.
    /// </summary>
    InvalidArguments,

    /// <summary>
    ///     General access denied error occurred.
    /// </summary>
    AccessDenied,

    /// <summary>
    ///     Operation timed out.
    /// </summary>
    Timeout,

    /// <summary>
    ///     Not enough memory to complete the operation.
    /// </summary>
    OutOfMemory
}
