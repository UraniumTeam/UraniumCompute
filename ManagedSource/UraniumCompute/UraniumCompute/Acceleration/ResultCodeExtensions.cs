namespace UraniumCompute.Acceleration;

public static class ResultCodeExtensions
{
    /// <summary>
    ///     Throw an <see cref="ErrorResultException" /> if the provided result code was not Success.
    /// </summary>
    /// <param name="resultCode">The result code to check.</param>
    /// <exception cref="ErrorResultException">The result was not Success.</exception>
    public static void ThrowOnError(this ResultCode resultCode)
    {
        if (resultCode == ResultCode.Success)
        {
            return;
        }

        throw new ErrorResultException(null, resultCode);
    }

    /// <summary>
    ///     Throw an <see cref="ErrorResultException" /> if the provided result code was not Success.
    /// </summary>
    /// <param name="resultCode">The result code to check.</param>
    /// <param name="message">The message to throw an exception with.</param>
    /// <exception cref="ErrorResultException">The result was not Success.</exception>
    public static void ThrowOnError(this ResultCode resultCode, string message)
    {
        if (resultCode == ResultCode.Success)
        {
            return;
        }

        throw new ErrorResultException(message, resultCode);
    }
}
