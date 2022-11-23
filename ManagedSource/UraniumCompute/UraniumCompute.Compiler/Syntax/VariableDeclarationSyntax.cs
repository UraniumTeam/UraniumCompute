using Mono.Cecil;
using UraniumCompute.Compiler.Disassembling;

namespace UraniumCompute.Compiler.Syntax;

internal sealed class VariableDeclarationStatementSyntax : StatementSyntax
{
    public TypeReference VariableType { get; }
    public string Name { get; }

    public VariableDeclarationStatementSyntax(TypeReference variableType, string name)
    {
        VariableType = variableType;
        Name = name;
    }

    public override string ToString()
    {
        return $"{Disassembler.ConvertType(VariableType)} {Name};";
    }
}
