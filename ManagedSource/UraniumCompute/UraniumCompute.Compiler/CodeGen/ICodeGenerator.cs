using UraniumCompute.Compiler.Syntax;

namespace UraniumCompute.Compiler.CodeGen;

internal interface ICodeGenerator
{
    TextWriter Output { get; }
    int IndentSize { get; }

    void EmitFunction(FunctionDeclarationSyntax syntax);
    string CreateForwardDeclaration(FunctionDeclarationSyntax syntax);
}
