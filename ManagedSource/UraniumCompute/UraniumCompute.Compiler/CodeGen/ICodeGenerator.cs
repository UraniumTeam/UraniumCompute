using Mono.Cecil;
using UraniumCompute.Compiler.Decompiling;
using UraniumCompute.Compiler.Syntax;

namespace UraniumCompute.Compiler.CodeGen;

internal interface ICodeGenerator
{
    TextWriter Output { get; }
    int IndentSize { get; }

    void EmitFunctionDeclaration(FunctionDeclarationSyntax syntax);
}
