namespace UraniumCompute.Acceleration;

/// <summary>
///     Represents a general result of a function call within the library.
///     Different functions may have their own result codes, but this enum should be enough for a general case.
/// </summary>
public enum ResultCode : uint
{
    /// Operation succeeded.
    Success,

    /// Operation failed.
    Fail,

    /// Operation aborted.
    Abort,

    /// Operation not implemented.
    NotImplemented,

    /// Operation was invalid.
    InvalidOperation,

    /// One or more arguments were invalid.
    InvalidArguments,

    /// General access denied error occurred.
    AccessDenied,

    /// Operation timed out.
    Timeout,

    /// Not enough memory to complete the operation.
    OutOfMemory
}
