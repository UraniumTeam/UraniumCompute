namespace UraniumCompute.Compiler.Decompiling;

internal sealed class FieldDesc
{
    public string Name { get; }
    public TypeSymbol FieldType { get; }

    public FieldDesc(string name, TypeSymbol fieldType)
    {
        Name = name;
        FieldType = fieldType;
    }
}
