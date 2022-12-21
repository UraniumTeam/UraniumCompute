using UraniumCompute.Compiler.Decompiling;

namespace UraniumCompute.Compiler.Syntax;

internal class ConstantsDeclarationSyntax : SyntaxNode
{
    public StructTypeSymbol CbufferType { get; }

    public ConstantsDeclarationSyntax(StructTypeSymbol cbufferType)
    {
        CbufferType = cbufferType;
    }

    public override string ToString()
    {
        return $"cbuffer {CbufferType.FullName} " +
               $"{{ {string.Join(" ", CbufferType.Fields.Select(x => $"{x.FieldType} {x.Name};"))} }};";
    }
}
