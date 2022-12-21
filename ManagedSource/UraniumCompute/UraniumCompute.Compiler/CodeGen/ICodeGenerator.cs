using UraniumCompute.Compiler.Syntax;

namespace UraniumCompute.Compiler.CodeGen;

internal interface ICodeGenerator
{
    TextWriter Output { get; }
    int IndentSize { get; }

    void EmitFunction(FunctionDeclarationSyntax syntax);
    void EmitStruct(StructDeclarationSyntax syntax);
    void EmitConstants(ConstantsDeclarationSyntax syntax);
    string CreateForwardDeclaration(FunctionDeclarationSyntax syntax);
    string CreateForwardDeclaration(StructDeclarationSyntax syntax);
}
