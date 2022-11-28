using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Mono.Cecil;
using UraniumCompute.Compiler.CodeGen;
using UraniumCompute.Compiler.Disassembling;
using UraniumCompute.Compiler.Syntax;

namespace UraniumCompute.Compiler.Decompiling;

public sealed class MethodCompilation
{
    private string MethodName { get; }
    private MethodDefinition MethodDefinition { get; }
    private KernelAttribute? Attribute { get; }

    private MethodCompilation(string methodName, KernelAttribute? attribute, MethodDefinition definition)
    {
        Attribute = attribute;
        MethodName = methodName;
        MethodDefinition = definition;
    }

    internal static string DecorateMethodName(string methodName)
    {
        var sb = new StringBuilder();
        sb.Append("un_user_func_");
        foreach (var c in methodName)
        {
            if (char.IsLetterOrDigit(c))
            {
                sb.Append(c);
            }
            else
            {
                sb.Append($"c_{(int)c:X}");
            }
        }

        return sb.ToString();
    }

    public static string Compile(Delegate method)
    {
        var results = new List<MethodCompilationResult>();
        var methods = new Stack<MethodCompilation>();
        var compiledMethods = new HashSet<MethodReference>();
        methods.Push(Create(method));

        while (methods.Any())
        {
            var m = methods.Pop();
            if (compiledMethods.Add(m.MethodDefinition))
            {
                var result = m.Compile(x =>
                    methods.Push(new MethodCompilation(DecorateMethodName(x.Name), null, x.Resolve())));
                results.Add(result);
            }
        }

        var declarations = results.Select(x => x.Declaration).Where(x => x is not null);
        var code = results.Select(x => x.Code!);
        return string.Join(Environment.NewLine, declarations.Concat(code));
    }

    private static MethodCompilation Create(Delegate d)
    {
        var type = d.Method.DeclaringType!;
        var a = AssemblyDefinition.ReadAssembly(type.Assembly.Location)!;
        var tr = a.MainModule.ImportReference(type)!;
        var td = tr.Resolve()!;

        var definition = td.Methods.Single(x => x.Name == d.Method.Name && x.Parameters.Count == d.Method.GetParameters().Length);
        var kernelAttribute = d.Method.GetCustomAttribute<KernelAttribute>() ?? new KernelAttribute();
        return new MethodCompilation("main", kernelAttribute, definition);
    }

    private MethodCompilationResult Compile(Action<MethodReference> userFunctionCallback)
    {
        var disassembler = Disassembler.Create(MethodDefinition);
        var disassemblyResult = disassembler.Disassemble();
        var syntaxTree = SyntaxTree.Create(userFunctionCallback, Attribute, disassemblyResult, MethodName);
        syntaxTree.Compile();
        syntaxTree = syntaxTree.Rewrite(SyntaxTree.GetStandardPasses());

        var textWriter = new StringWriter();
        var codeGenerator = new HlslCodeGenerator(textWriter, 4);
        syntaxTree.GenerateCode(codeGenerator);

        var declaration = !syntaxTree.Function?.IsEntryPoint ?? false
            ? codeGenerator.CreateForwardDeclaration(syntaxTree.Function!)
            : null;
        return new MethodCompilationResult(textWriter.ToString(), declaration, Array.Empty<Diagnostic>());
    }
}
