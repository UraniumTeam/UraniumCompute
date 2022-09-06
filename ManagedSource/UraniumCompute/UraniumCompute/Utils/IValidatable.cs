using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace UraniumCompute.Utils;

/// <summary>
///     An interface for structs that can be validated.
/// </summary>
public interface IValidatable
{
    [Pure]
    bool HasValidationErrors([MaybeNullWhen(false)] out string errorMessage);
}
