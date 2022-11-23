using System.Diagnostics;
using UraniumCompute.Compiler.Syntax;

namespace UraniumCompute.Compiler.Rewriting;

internal sealed class BranchResolver : ISyntaxTreeRewriter
{
    private readonly List<StatementSyntax> statements = new();

    public SyntaxTree Rewrite(SyntaxTree syntaxTree)
    {
        var cfg = ControlFlowGraph.Create(syntaxTree.Function!.Block);
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
                var secondaryBranch = block.Outgoing.Single(x => !x.IsPrimary);

                var commonBlock = FindFirstCommonBlock(block.Outgoing[0].To, block.Outgoing[1].To);
                stopBlocks.Push(commonBlock);
                stopBlocks.Push(commonBlock);

                var thenBlock = new BlockStatementSyntax(WriteBlock(primaryBranch.To, stopBlocks));
                var elseBlock = new BlockStatementSyntax(WriteBlock(secondaryBranch.To, stopBlocks));
                var elseClause = elseBlock.Statements.Any()
                    ? new ElseClauseSyntax(elseBlock)
                    : null;

                yield return new IfStatementSyntax(primaryBranch.Condition!, thenBlock, elseClause);

                foreach (var statement in WriteBlock(commonBlock, stopBlocks))
                {
                    yield return statement;
                }

                break;
        }
    }

    private static ControlFlowGraph.BasicBlock FindFirstCommonBlock(ControlFlowGraph.BasicBlock leftBranch,
        ControlFlowGraph.BasicBlock rightBranch)
    {
        var visited = new HashSet<ControlFlowGraph.BasicBlock>();
        var left = ControlFlowGraph.BreadthSearch(leftBranch).ToArray();
        var right = ControlFlowGraph.BreadthSearch(rightBranch).ToArray();

        var i = 0;
        for (; i < Math.Min(left.Length, right.Length); ++i)
        {
            if (!visited.Add(left[i]))
            {
                return left[i];
            }

            if (!visited.Add(right[i]))
            {
                return right[i];
            }
        }

        foreach (var block in left.Skip(i).Concat(right.Skip(i)))
        {
            if (!visited.Add(block))
            {
                return block;
            }
        }

        throw new Exception("Couldn't find common block");
    }
}
