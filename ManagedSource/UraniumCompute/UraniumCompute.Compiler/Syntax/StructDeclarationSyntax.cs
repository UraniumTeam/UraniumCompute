using UraniumCompute.Compiler.Decompiling;

namespace UraniumCompute.Compiler.Syntax;

internal sealed class StructDeclarationSyntax : SyntaxNode
{
    public StructTypeSymbol StructType { get; }

    public StructDeclarationSyntax(StructTypeSymbol structType)
    {
        StructType = structType;
    }

    public override string ToString()
    {
        return $"struct {StructType.FullName} " +
               $"{{ {string.Join(" ", StructType.Fields.Select(x => $"{x.FieldType} {x.Name};"))} }};";
    }
}
