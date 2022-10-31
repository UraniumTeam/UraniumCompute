using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;

namespace UraniumCompute.Compiler.Disassembling;

internal class DisassemblyResult
{
    public IEnumerable<Instruction> Instructions { get; }

    public string Name { get; }

    public Collection<ParameterDefinition> Parameters { get; }

    public TypeReference ReturnType { get; }

    public Collection<VariableDefinition> Variables { get; }

    public DisassemblyResult(
        IEnumerable<Instruction> instructions, 
        string name, 
        Collection<ParameterDefinition> parameters, 
        TypeReference returnType,
        Collection<VariableDefinition> variables)
    {
        Instructions = instructions;
        Name = name;
        Parameters = parameters;
        ReturnType = returnType;
        Variables = variables;
    }
}