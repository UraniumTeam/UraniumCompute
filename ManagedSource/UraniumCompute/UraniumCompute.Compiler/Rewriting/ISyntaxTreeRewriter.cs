using UraniumCompute.Compiler.Syntax;

namespace UraniumCompute.Compiler.Rewriting;

internal interface ISyntaxTreeRewriter
{
    SyntaxTree Rewrite(SyntaxTree syntaxTree);
}
