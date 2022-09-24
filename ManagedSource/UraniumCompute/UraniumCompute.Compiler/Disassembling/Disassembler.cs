using Mono.Cecil;
using Mono.Cecil.Cil;

namespace UraniumCompute.Compiler.Disassembling;

internal sealed class Disassembler
{
    public MethodDefinition MethodDefinition { get; }

    private Disassembler(MethodDefinition methodDefinition)
    {
        MethodDefinition = methodDefinition;
    }

    public static Disassembler Create(MethodDefinition methodDefinition)
    {
        return new Disassembler(methodDefinition);
    }

    public IEnumerable<Instruction> Disassemble()
    {
        return MethodDefinition.Body.Instructions;
    }
}
