using UraniumCompute.Acceleration;

namespace UraniumCompute.Utils;

public static class AdapterSpanExtensions
{
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

    public static ref readonly AdapterInfo FirstDiscrete(this ReadOnlySpan<AdapterInfo> adapters)
    {
        return ref adapters.FirstOfKind(AdapterKind.Discrete);
    }
}
