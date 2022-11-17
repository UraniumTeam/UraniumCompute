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

    private static string ConvertPrimitiveType(TypeReference tr)
    {
        // TODO: handle errors without exceptions (use Diagnostics)
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

    internal static string ConvertType(TypeReference tr)
    {
        Debug.Assert(tr.IsPrimitive || tr.IsGenericInstance, "Type isn't supported yet...");

        if (tr.IsGenericInstance)
        {
            var instance = (GenericInstanceType)tr;

            if (instance.Namespace == "System")
                return instance.Name switch
                {
                    "Span`1" => $"RWStructuredBuffer<{ConvertPrimitiveType(instance.GenericArguments[0])}>",
                    _ => throw new Exception($"Unknown generic type: {instance.Name}")
                };
            throw new Exception($"Unknown namespace: {instance.Namespace}");
        }

        return ConvertPrimitiveType(tr);
    }
}
