using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace UraniumCompute.Utils;

public interface IValidatable
{
    [Pure]
    bool HasValidationErrors([MaybeNullWhen(false)] out string errorMessage);
}
