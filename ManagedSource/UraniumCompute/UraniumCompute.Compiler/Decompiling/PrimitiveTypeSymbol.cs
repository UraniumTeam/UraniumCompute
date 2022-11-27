namespace UraniumCompute.Compiler.Decompiling;

internal sealed class PrimitiveTypeSymbol : TypeSymbol
{
    public override string FullName { get; }

    public PrimitiveTypeSymbol(string name)
    {
        FullName = name;
    }
}
