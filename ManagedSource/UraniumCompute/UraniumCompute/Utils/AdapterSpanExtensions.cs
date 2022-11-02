using UraniumCompute.Acceleration;

namespace UraniumCompute.Utils;

public static class AdapterSpanExtensions
{
    /// <summary>
    ///     Get first adapter of specified kind or null.
    /// </summary>
    /// <param name="adapters">An adapter collection to search in.</param>
    /// <param name="kind">The kind to search for.</param>
    /// <returns>The found adapter or null.</returns>
    public static AdapterInfo? FirstOfKindOrNull(this ReadOnlySpan<AdapterInfo> adapters, AdapterKind kind)
    {
        foreach (ref readonly var adapter in adapters)
        {
            if (adapter.Kind == kind)
            {
                return adapter;
            }
        }

        return null;
    }

    /// <summary>
    ///     Get first adapter of specified kind.
    /// </summary>
    /// <param name="adapters">An adapter collection to search in.</param>
    /// <param name="kind">The kind to search for.</param>
    /// <returns>The found adapter.</returns>
    /// <exception cref="ArgumentException">Adapter of the specified kind was not in the collection.</exception>
    public static ref readonly AdapterInfo FirstOfKind(this ReadOnlySpan<AdapterInfo> adapters, AdapterKind kind)
    {
        foreach (ref readonly var adapter in adapters)
        {
            if (adapter.Kind == kind)
            {
                return ref adapter;
            }
        }

        throw new ArgumentException($"Adapter of kind {kind} was not found");
    }

    /// <summary>
    ///     Get first discrete adapter.
    /// </summary>
    /// <param name="adapters">An adapter collection to search in.</param>
    /// <returns>The found adapter.</returns>
    /// <exception cref="ArgumentException">There was no discrete adapters in the collection.</exception>
    public static ref readonly AdapterInfo FirstDiscrete(this ReadOnlySpan<AdapterInfo> adapters)
    {
        return ref adapters.FirstOfKind(AdapterKind.Discrete);
    }

    /// <summary>
    ///     Get first discrete adapter or null.
    /// </summary>
    /// <param name="adapters">An adapter collection to search in.</param>
    /// <returns>The found adapter or null.</returns>
    public static AdapterInfo? FirstDiscreteOrNull(this ReadOnlySpan<AdapterInfo> adapters)
    {
        return adapters.FirstOfKindOrNull(AdapterKind.Discrete);
    }
}
