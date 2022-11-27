namespace UraniumCompute.Compiler.Decompiling;

internal sealed class UserFunctionSymbol : FunctionSymbol
{
    public override string FullName { get; }
    public override TypeSymbol ReturnType { get; }
    public override TypeSymbol[] ArgumentTypes { get; }

    public UserFunctionSymbol(string fullName, TypeSymbol returnType, IEnumerable<TypeSymbol> argumentTypes)
    {
        FullName = MethodCompilation.DecorateMethodName(fullName);
        ReturnType = returnType;
        ArgumentTypes = argumentTypes.ToArray();
    }
}
