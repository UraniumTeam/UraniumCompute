namespace UraniumCompute.Acceleration;

public static class ResultCodeExtensions
{
    public static void ThrowOnError(this ResultCode resultCode)
    {
        if (resultCode == ResultCode.Success)
        {
            return;
        }

        throw new ErrorResultException(null, resultCode);
    }

    public static void ThrowOnError(this ResultCode resultCode, string message)
    {
        if (resultCode == ResultCode.Success)
        {
            return;
        }

        throw new ErrorResultException(message, resultCode);
    }
}
