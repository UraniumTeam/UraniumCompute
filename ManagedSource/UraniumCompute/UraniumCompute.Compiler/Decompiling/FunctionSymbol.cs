namespace UraniumCompute.Compiler.Decompiling;

internal abstract class FunctionSymbol
{
    public abstract string FullName { get; }
    public abstract TypeSymbol ReturnType { get; }
    public abstract TypeSymbol[] ArgumentTypes { get; }
}
