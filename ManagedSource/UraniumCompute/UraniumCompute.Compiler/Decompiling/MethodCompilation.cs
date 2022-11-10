using Mono.Cecil;
using UraniumCompute.Compiler.Disassembling;
using UraniumCompute.Compiler.Syntax;

namespace UraniumCompute.Compiler.Decompiling;

public sealed class MethodCompilation
{
    private MethodDefinition MethodDefinition { get; }

    private MethodCompilation(MethodDefinition definition)
    {
        MethodDefinition = definition;
    }

    public static MethodCompilation Create(Delegate d)
    {
        var type = d.Method.DeclaringType!;
        var a = AssemblyDefinition.ReadAssembly(type.Assembly.Location)!;
        var tr = a.MainModule.ImportReference(type)!;
        var td = tr.Resolve()!;
        
        return new MethodCompilation(td.Methods.Single(x => 
            x.Name == d.Method.Name
            && x.Parameters.Count == d.Method.GetParameters().Length));
    }

    public MethodCompilationResult Compile()
    {
        // 1. Disassemble the method IL using the Disassembler class
        // 2. Create a syntax tree from classes derived from SyntaxNode
        // 3. Generate code and aggregate diagnostics
        var disassembler = Disassembler.Create(MethodDefinition);
        var disassemblyResult = disassembler.Disassemble();
        var syntaxTree = SyntaxTree.Create(disassemblyResult);
        syntaxTree.Compile();

        return new MethodCompilationResult(syntaxTree.ToString(), Array.Empty<Diagnostic>());
    }
}
