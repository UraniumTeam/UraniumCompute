using System.Diagnostics;
using UraniumCompute.Compiler.Syntax;

namespace UraniumCompute.Compiler.Rewriting;

internal sealed class BranchResolver : ISyntaxTreeRewriter
{
    private readonly List<StatementSyntax> statements = new();

    private readonly Stack<ControlFlowGraph.BasicBlock> stopBlocks = new();
    private readonly HashSet<ControlFlowGraph.BasicBlock> outerLoops = new();
    private readonly HashSet<ControlFlowGraph.BasicBlock> visited = new();

    public SyntaxTree Rewrite(SyntaxTree syntaxTree)
    {
        var cfg = ControlFlowGraph.Create(syntaxTree.Function!.Block);
        statements.AddRange(WriteBlock(cfg.Start));
        return syntaxTree.WithStatements(statements);
    }

    private IEnumerable<StatementSyntax> WriteBlock(ControlFlowGraph.BasicBlock block)
    {
        if (stopBlocks.Any() && block == stopBlocks.Peek())
        {
            stopBlocks.Pop();
            return Enumerable.Empty<StatementSyntax>();
        }

        if (!visited.Add(block))
        {
            return Enumerable.Empty<StatementSyntax>();
        }

        switch (block.Outgoing.Count)
        {
            case 0:
                return FilterStatements(block.Statements);
            case 1:
                var branch = block.Outgoing.Single();
                Debug.Assert(branch.Condition is null);
                return FilterStatements(block.Statements)
                    .Concat(WriteBlock(branch.To));
            case 2:
                Debug.Assert(block.Outgoing.All(x => x.Condition is not null));
                var primaryBranch = block.Outgoing.Single(x => x.IsPrimary);
                var secondaryBranch = block.Outgoing.Single(x => !x.IsPrimary);

                var commonBlock = FindFirstCommonBlock(block.Outgoing[0].To, block.Outgoing[1].To);

                var primaryLoop = DetectLoop(primaryBranch.To, block, commonBlock);
                var secondaryLoop = DetectLoop(secondaryBranch.To, block, commonBlock);
                if (!primaryLoop && !secondaryLoop)
                {
                    stopBlocks.Push(commonBlock);
                    stopBlocks.Push(commonBlock);

                    var thenBlock = new BlockStatementSyntax(WriteBlock(primaryBranch.To));
                    var elseBlock = new BlockStatementSyntax(WriteBlock(secondaryBranch.To));
                    var elseClause = elseBlock.Statements.Any()
                        ? new ElseClauseSyntax(elseBlock)
                        : null;

                    return FilterStatements(block.Statements)
                        .Append(new IfStatementSyntax(primaryBranch.Condition!, thenBlock, elseClause))
                        .Concat(WriteBlock(commonBlock));
                }

                outerLoops.Add(block);
                stopBlocks.Push(block);
                var loopBranch = primaryLoop ? primaryBranch : secondaryBranch;
                var otherBranch = secondaryLoop ? primaryBranch : secondaryBranch;
                var loopBreak = new IfStatementSyntax(otherBranch.Condition!,
                    new BlockStatementSyntax(new BreakStatementSyntax()), null);
                var loopBlock = new BlockStatementSyntax(FilterStatements(block.Statements)
                    .Concat(Enumerable.Repeat<StatementSyntax>(loopBreak, 1))
                    .Concat(WriteBlock(loopBranch.To)));
                outerLoops.Remove(block);

                return Enumerable.Repeat(WhileStatementSyntax.CreateInfinite(loopBlock), 1)
                    .Concat(WriteBlock(otherBranch.To));
            default:
                throw new InvalidOperationException($"A basic block has {block.Outgoing.Count} (more than 2) branches");
        }
    }

    private static IEnumerable<StatementSyntax> FilterStatements(IEnumerable<StatementSyntax> statements)
    {
        return statements.Where(statement =>
            statement is not (LabelStatementSyntax or GotoStatementSyntax or ConditionalGotoStatementSyntax));
    }

    private bool DetectLoop(ControlFlowGraph.BasicBlock block, ControlFlowGraph.BasicBlock parent,
        ControlFlowGraph.BasicBlock stopBlock)
    {
        foreach (var basicBlock in ControlFlowGraph.BreadthSearch(block))
        {
            if (outerLoops.Contains(basicBlock))
            {
                return false;
            }

            if (basicBlock == parent)
            {
                return true;
            }

            if (basicBlock == stopBlock)
            {
                return false;
            }
        }

        return false;
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
