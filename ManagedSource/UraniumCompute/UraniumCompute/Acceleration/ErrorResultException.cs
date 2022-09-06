namespace UraniumCompute.Acceleration;

/// <summary>
///     Indicates that an unmanaged function returned a <see cref="ResultCode" /> other than Success.
/// </summary>
public class ErrorResultException : Exception
{
    /// <summary>
    ///     The error code.
    /// </summary>
    public ResultCode ResultCode { get; }

    /// <summary>
    ///     Create a new instance of <see cref="ErrorResultException" />.
    /// </summary>
    /// <param name="message">Exception message.</param>
    /// <param name="resultCode">Error code.</param>
    public ErrorResultException(string? message, ResultCode resultCode) : base(
        message is null
            ? $"[ResultCode was {resultCode}]"
            : $"[ResultCode was {resultCode}]: {message}")
    {
        ResultCode = resultCode;
    }
}
