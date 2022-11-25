namespace UraniumCompute.Compiler.Decompiling;

internal abstract class TypeSymbol
{
    public abstract string FullName { get; }

    public override string ToString()
    {
        return FullName;
    }
}
