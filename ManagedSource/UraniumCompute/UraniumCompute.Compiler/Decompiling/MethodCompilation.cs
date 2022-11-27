using System.Reflection;
using Mono.Cecil;
using UraniumCompute.Compiler.CodeGen;
using UraniumCompute.Compiler.Disassembling;
using UraniumCompute.Compiler.Syntax;

namespace UraniumCompute.Compiler.Decompiling;

public sealed class MethodCompilation
{
    private string MethodName { get; }
    private MethodDefinition MethodDefinition { get; }
    private MethodInfo MethodInfo { get; }

    private MethodCompilation(string methodName, MethodInfo methodInfo, MethodDefinition definition)
    {
        MethodInfo = methodInfo;
        MethodName = methodName;
        MethodDefinition = definition;
    }

    public static MethodCompilation Create(Delegate d, string methodName = "main")
    {
        var type = d.Method.DeclaringType!;
        var a = AssemblyDefinition.ReadAssembly(type.Assembly.Location)!;
        var tr = a.MainModule.ImportReference(type)!;
        var td = tr.Resolve()!;

        var definition = td.Methods.Single(x => x.Name == d.Method.Name && x.Parameters.Count == d.Method.GetParameters().Length);
        return new MethodCompilation(methodName, d.Method, definition);
    }

    public MethodCompilationResult Compile()
    {
        var disassembler = Disassembler.Create(MethodDefinition);
        var disassemblyResult = disassembler.Disassemble();
        var syntaxTree = SyntaxTree.Create(MethodInfo, disassemblyResult, MethodName);
        syntaxTree.Compile();
        syntaxTree = syntaxTree.Rewrite(SyntaxTree.GetStandardPasses());

        var textWriter = new StringWriter();
        var codeGenerator = new HlslCodeGenerator(textWriter, 4);
        syntaxTree.GenerateCode(codeGenerator);

        return new MethodCompilationResult(textWriter.ToString(), Array.Empty<Diagnostic>());
    }
}
