using Mono.Cecil;
using UraniumCompute.Compiler.Decompiling;

namespace UraniumCompute.Compiler.Syntax;

internal sealed class VariableDeclarationStatementSyntax : StatementSyntax
{
    public TypeSymbol VariableType { get; }
    public string Name { get; }

    public VariableDeclarationStatementSyntax(TypeSymbol variableType, string name)
    {
        VariableType = variableType;
        Name = name;
    }

    public override string ToString()
    {
        return $"{VariableType} {Name};";
    }
}
