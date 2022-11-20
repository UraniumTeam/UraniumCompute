using System.Diagnostics;
using UraniumCompute.Compiler.Syntax;

namespace UraniumCompute.Compiler.Rewriting;

internal sealed class BranchResolver : ISyntaxTreeRewriter
{
    public SyntaxTree Rewrite(SyntaxTree syntaxTree)
    {
        var cfg = ControlFlowGraph.Create(syntaxTree.Statements);
        return syntaxTree.WithStatements(syntaxTree.Statements);
    }
}
