using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UraniumCompute.Generator;

internal class CompiledMethod
{
    public MethodDeclarationSyntax Declaration { get; }
    public string Name { get; }
    public BlockSyntax Body { get; }

    public CompiledMethod(MethodDeclarationSyntax method)
    {
        Declaration = method;
        Name = method.Identifier.Text;
        Body = method.Body!;
    }
}
