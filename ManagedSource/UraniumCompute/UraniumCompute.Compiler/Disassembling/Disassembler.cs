using Mono.Cecil;

namespace UraniumCompute.Compiler.Disassembling;

internal sealed class Disassembler
{
    private MethodDefinition MethodDefinition { get; }

    private Disassembler(MethodDefinition methodDefinition)
    {
        MethodDefinition = methodDefinition;
    }

    public static Disassembler Create(MethodDefinition methodDefinition)
    {
        return new Disassembler(methodDefinition);
    }

    public DisassemblyResult Disassemble()
    {
        return new DisassemblyResult(
            MethodDefinition.Body.Instructions,
            MethodDefinition.Name,
            MethodDefinition.Parameters,
            MethodDefinition.ReturnType,
            MethodDefinition.Body.Variables
        );
    }
}
