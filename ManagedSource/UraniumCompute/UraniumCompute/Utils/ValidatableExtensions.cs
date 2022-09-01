using System.Runtime.CompilerServices;

namespace UraniumCompute.Utils;

public static class ValidatableExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ThrowOnError<T>(this T value)
        where T : IValidatable
    {
        if (value.HasValidationErrors(out var message))
        {
            throw new ArgumentException($"Arguments in {typeof(T)} have errors: {message}");
        }
    }
}
