using System.Diagnostics;
using UraniumCompute.Compiler.Syntax;

namespace UraniumCompute.Compiler.Rewriting;

internal sealed class BranchResolver : ISyntaxTreeRewriter
{
    private readonly List<StatementSyntax> statements = new();

    public SyntaxTree Rewrite(SyntaxTree syntaxTree)
    {
        var cfg = ControlFlowGraph.Create(syntaxTree.Block);
        statements.AddRange(WriteBlock(cfg.Start, new Stack<ControlFlowGraph.BasicBlock>()));
        return syntaxTree.WithStatements(statements);
    }

    private static IEnumerable<StatementSyntax> WriteBlock(ControlFlowGraph.BasicBlock block,
        Stack<ControlFlowGraph.BasicBlock> stopBlocks)
    {
        if (stopBlocks.Any() && block == stopBlocks.Peek())
        {
            stopBlocks.Pop();
            yield break;
        }

        foreach (var statement in block.Statements)
        {
            if (statement is LabelStatementSyntax or GotoStatementSyntax or ConditionalGotoStatementSyntax)
            {
                continue;
            }

            yield return statement;
        }

        switch (block.Outgoing.Count)
        {
            case 0:
                yield break;
            case 1:
                var branch = block.Outgoing.Single();
                Debug.Assert(branch.Condition is null);

                foreach (var statement in WriteBlock(branch.To, stopBlocks))
                {
                    yield return statement;
                }

                break;
            case 2:
                Debug.Assert(block.Outgoing.All(x => x.Condition is not null));
                var primaryBranch = block.Outgoing.Single(x => x.IsPrimary);
                var secondaryBranch = block.Outgoing.Single(x => !x.IsPrimary).To;

                stopBlocks.Push(secondaryBranch);
                yield return new IfStatementSyntax(primaryBranch.Condition!,
                    new BlockStatementSyntax(WriteBlock(primaryBranch.To, stopBlocks)),
                    null);

                foreach (var statement in WriteBlock(secondaryBranch, stopBlocks))
                {
                    yield return statement;
                }

                break;
        }
    }
}
