using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;

namespace UraniumCompute.Compiler.Disassembling;

internal class DisassemblyResult
{
    public IEnumerable<Instruction> Instructions { get; }
    public Collection<ParameterDefinition> Parameters { get; }
    public TypeReference ReturnType { get; }
    public Collection<VariableDefinition> Variables { get; }
    public MethodDefinition MethodDefinition { get; }
    public bool HasThis => MethodDefinition.HasThis;

    public DisassemblyResult(
        IEnumerable<Instruction> instructions,
        Collection<ParameterDefinition> parameters,
        TypeReference returnType,
        Collection<VariableDefinition> variables,
        MethodDefinition methodDefinition)
    {
        Instructions = instructions;
        Parameters = parameters;
        ReturnType = returnType;
        Variables = variables;
        MethodDefinition = methodDefinition;
    }
}
