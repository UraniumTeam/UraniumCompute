using Mono.Cecil;
using System.Diagnostics;

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
    
    internal static string ConvertType(TypeReference tr)
    {
        // TODO: handle errors without exceptions (use Diagnostics)
        Debug.Assert(tr.IsPrimitive, "Non-primitive types aren't supported yet...");
        return tr.Name switch
        {
            "Void" => "void",
            nameof(Byte) or nameof(SByte) =>
                throw new InvalidOperationException("8-bit ints are not supported by GPU"),
            nameof(Int16) or nameof(UInt16) =>
                throw new InvalidOperationException("16-bit ints are not supported by GPU"),
            nameof(Int64) or nameof(UInt64) =>
                throw new InvalidOperationException("64-bit ints are not supported by GPU"),
            nameof(Int32) => "int",
            nameof(UInt32) => "uint",
            nameof(Single) => "float",
            nameof(Double) => "double",
            nameof(Boolean) => "bool",
            _ => throw new Exception($"Unknown type: {tr.Name}")
        };
    }
}
