namespace UraniumCompute.Acceleration;

public class ErrorResultException : Exception
{
    public ResultCode ResultCode { get; }

    public ErrorResultException(string? message, ResultCode resultCode) : base(
        message is null
            ? $"[ResultCode was {resultCode}]"
            : $"[ResultCode was {resultCode}]: {message}")
    {
        ResultCode = resultCode;
    }
}
