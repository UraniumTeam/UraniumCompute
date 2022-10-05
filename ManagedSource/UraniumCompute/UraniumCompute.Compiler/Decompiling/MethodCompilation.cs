using Mono.Cecil;
using UraniumCompute.Compiler.Disassembling;

namespace UraniumCompute.Compiler.Decompiling;

public sealed class MethodCompilation
{
    internal MethodDefinition MethodDefinition { get; }

    private MethodCompilation(MethodDefinition definition)
    {
        MethodDefinition = definition;
    }

    public static MethodCompilation Create(Type type, string methodName)
    {
        var a = AssemblyDefinition.ReadAssembly(type.Assembly.Location)!;
        var tr = a.MainModule.ImportReference(type)!;
        var td = tr.Resolve()!;

        // TODO: this code can choose a wrong overload, so we assert the unique name for now
        var method = td.Methods.SingleOrDefault(x => x.Name == methodName);
        if (method is null)
        {
            throw new ArgumentException($"The requested method {methodName} was not found in the {type} type " +
                                        "or had multiple overloads (which are currently not supported)");
        }

        return new MethodCompilation(method);
    }

    public MethodCompilationResult Compile()
    {
        // 1. Disassemble the method IL using the Disassembler class
        // 2. Create a syntax tree from classes derived from SyntaxNode
        // 3. Generate code and aggregate diagnostics
        var disassembler = Disassembler.Create(MethodDefinition);
        foreach (var instruction in disassembler.Disassemble())
        {
            Console.WriteLine(instruction);
        }

        throw new NotImplementedException();
    }
}
