using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Text;

namespace UraniumCompute.Generator;

[Generator(LanguageNames.CSharp)]
public class SourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var methodDeclarations = context.SyntaxProvider.CreateSyntaxProvider(
            static (s, _) => IsMethodWithAttributes(s),
            static (ctx, _) => GetMethodDeclaration(ctx));

        context.RegisterSourceOutput(
            methodDeclarations.Collect(),
            static (spc, source) => Execute(spc, source));
    }

    static bool IsMethodWithAttributes(SyntaxNode node)
        => node is MethodDeclarationSyntax { AttributeLists.Count: > 0 } method
           && method.AttributeLists
               .SelectMany(als => als.Attributes)
               .Any(a => $"{a.Name}Attribute" == nameof(CompileKernelAttribute));

    static MethodDeclarationSyntax GetMethodDeclaration(GeneratorSyntaxContext context)
        => (MethodDeclarationSyntax)context.Node;

    static void Execute(
        SourceProductionContext context,
        ImmutableArray<MethodDeclarationSyntax> methods)
    {
        if (methods.IsDefaultOrEmpty)
        {
            throw new Exception("There are no methods marked with the CompileKernel attribute");
        }
        
        var userMethods = GetUserMethods(methods, context.CancellationToken);
        var hlslCode = new CSharpToHlslTranslator(userMethods).GenerateHlslCode();
        // context.AddSource("CompiledUserMethods.g.hlsl", SourceText.From(hlslCode, Encoding.UTF8));
        // генератор создает файл .cs,
        // который начинает участвовать в дальнейшей компиляции,
        // но, т.к. файл содержит не C# код, появляются ошибки, 
        // мешающие дальнейшей компиляции
    }

    static List<CompiledMethod> GetUserMethods(
        ImmutableArray<MethodDeclarationSyntax> methods,
        CancellationToken ct)
    {
        var userMethods = new List<CompiledMethod>();
        foreach (var method in methods)
        {
            ct.ThrowIfCancellationRequested();
            userMethods.Add(new CompiledMethod(method));
        }
        return userMethods;
    }
}
