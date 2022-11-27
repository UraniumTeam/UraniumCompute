namespace UraniumCompute.Compiler.Decompiling;

[AttributeUsage(AttributeTargets.Method)]
public sealed class KernelAttribute : Attribute
{
    public int X { get; init; } = 1;
    public int Y { get; init; } = 1;
    public int Z { get; init; } = 1;

    public override string ToString()
    {
        return $"[numthreads({X}, {Y}, {Z})]";
    }
}
