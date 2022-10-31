using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;

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

    public IEnumerable<Instruction> GetInstructions()
    {
        return MethodDefinition.Body.Instructions;
    }
    
    public string GetName()
    {
        return MethodDefinition.Name;
    }
    
    public string GetFullName()
    {
        return MethodDefinition.FullName;
    }
    
    public Collection<ParameterDefinition> Parameters()
    {
        return MethodDefinition.Parameters;
    }
    
    public TypeReference GetReturnType()
    {
        return MethodDefinition.ReturnType;
    }
    
}
