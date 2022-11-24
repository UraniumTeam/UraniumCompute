using System.Text;
using UraniumCompute.Compiler.Syntax;

namespace UraniumCompute.Compiler.Rewriting;

internal sealed class ControlFlowGraph
{
    public BasicBlock Start { get; }
    public BasicBlock End { get; }
    public List<BasicBlock> Blocks { get; }
    public List<BasicBlockBranch> Branches { get; }

    public static ControlFlowGraph Create(BlockStatementSyntax block)
    {
        var basicBlockBuilder = new BasicBlockBuilder();
        var blocks = basicBlockBuilder.Build(block);

        var graphBuilder = new GraphBuilder();
        return graphBuilder.Build(blocks);
    }

    private ControlFlowGraph(BasicBlock start, BasicBlock end, List<BasicBlock> blocks,
        List<BasicBlockBranch> branches)
    {
        Start = start;
        End = end;
        Blocks = blocks;
        Branches = branches;
    }

    public sealed class BasicBlock
    {
        public bool IsStart { get; }
        public bool IsEnd { get; }
        public List<StatementSyntax> Statements { get; } = new();
        public List<BasicBlockBranch> Incoming { get; } = new();
        public List<BasicBlockBranch> Outgoing { get; } = new();

        public BasicBlock()
        {
        }

        public BasicBlock(bool isStart)
        {
            IsStart = isStart;
            IsEnd = !isStart;
        }

        public override string ToString()
        {
            if (IsStart)
            {
                return "<Start>";
            }

            if (IsEnd)
            {
                return "<End>";
            }

            var sb = new StringBuilder();
            foreach (var statement in Statements)
            {
                sb.Append($"{statement} ");
            }

            return sb.ToString();
        }
    }

    public sealed class BasicBlockBranch
    {
        public BasicBlock From { get; }
        public BasicBlock To { get; }
        public ExpressionSyntax? Condition { get; }
        public bool IsPrimary { get; }

        public BasicBlockBranch(BasicBlock from, BasicBlock to, ExpressionSyntax? condition, bool isPrimary)
        {
            From = from;
            To = to;
            Condition = condition;
            IsPrimary = isPrimary;
        }

        public override string ToString()
        {
            return Condition?.ToString() ?? string.Empty;
        }
    }

    private sealed class BasicBlockBuilder
    {
        private readonly List<StatementSyntax> statements = new();
        private readonly List<BasicBlock> blocks = new();

        public List<BasicBlock> Build(BlockStatementSyntax block)
        {
            foreach (var statement in block.Statements)
            {
                switch (statement)
                {
                    case LabelStatementSyntax label:
                        StartBlock();
                        statements.Add(label);
                        break;
                    case ControlFlowStatement controlFlowStatement:
                        statements.Add(controlFlowStatement);
                        StartBlock();
                        break;
                    default:
                        statements.Add(statement);
                        break;
                }
            }

            EndBlock();

            return blocks.ToList();
        }

        private void StartBlock()
        {
            EndBlock();
        }

        private void EndBlock()
        {
            if (statements.Any())
            {
                var block = new BasicBlock();
                block.Statements.AddRange(statements);
                blocks.Add(block);
                statements.Clear();
            }
        }
    }

    private sealed class GraphBuilder
    {
        private readonly Dictionary<StatementSyntax, BasicBlock> blockFromStatement = new();

        private readonly Dictionary<int, BasicBlock> blockFromOffset = new();

        private readonly List<BasicBlockBranch> branches = new();
        private readonly BasicBlock start = new(true);
        private readonly BasicBlock end = new(false);

        public ControlFlowGraph Build(List<BasicBlock> blocks)
        {
            if (!blocks.Any())
            {
                Connect(start, end);
            }
            else
            {
                Connect(start, blocks.First());
            }

            foreach (var block in blocks)
            {
                foreach (var statement in block.Statements)
                {
                    blockFromStatement.Add(statement, block);
                    if (statement is LabelStatementSyntax label)
                    {
                        blockFromOffset.Add(label.Offset, block);
                    }
                }
            }

            for (var i = 0; i < blocks.Count; i++)
            {
                var current = blocks[i];
                var next = i == blocks.Count - 1 ? end : blocks[i + 1];

                foreach (var statement in current.Statements)
                {
                    var isLastStatementInBlock = statement == current.Statements.Last();
                    switch (statement)
                    {
                        case GotoStatementSyntax gs:
                            Connect(current, blockFromOffset[gs.Offset]);
                            break;
                        case ConditionalGotoStatementSyntax cgs:
                            Connect(current, blockFromOffset[cgs.Offset], cgs.Condition);
                            Connect(current, next, cgs.Condition, true);
                            break;
                        case ReturnStatementSyntax:
                            Connect(current, end);
                            break;
                        default:
                            if (isLastStatementInBlock)
                            {
                                Connect(current, next);
                            }

                            break;
                    }
                }
            }

            var scanAgain = true;
            while (scanAgain)
            {
                scanAgain = false;
                foreach (var block in blocks.Where(block => !block.Incoming.Any()))
                {
                    RemoveBlock(blocks, block);
                    scanAgain = true;
                    break;
                }
            }

            blocks.Insert(0, start);
            blocks.Add(end);

            return new ControlFlowGraph(start, end, blocks, branches);
        }

        private void Connect(BasicBlock from, BasicBlock to, ExpressionSyntax? condition = null, bool isPrimary = false)
        {
            if (condition is LiteralExpressionSyntax l)
            {
                var value = (bool)l.Value;
                if (value)
                {
                    condition = null;
                }
                else
                {
                    return;
                }
            }

            if (isPrimary)
            {
                condition = Negate(condition!);
            }

            var branch = new BasicBlockBranch(from, to, condition, isPrimary);
            from.Outgoing.Add(branch);
            to.Incoming.Add(branch);
            branches.Add(branch);
        }

        private void RemoveBlock(List<BasicBlock> blocks, BasicBlock block)
        {
            foreach (var branch in block.Incoming)
            {
                branch.From.Outgoing.Remove(branch);
                branches.Remove(branch);
            }

            foreach (var branch in block.Outgoing)
            {
                branch.To.Incoming.Remove(branch);
                branches.Remove(branch);
            }

            blocks.Remove(block);
        }

        private static ExpressionSyntax Negate(ExpressionSyntax condition)
        {
            if (condition is LiteralExpressionSyntax literal)
            {
                var value = (bool)literal.Value;
                return new LiteralExpressionSyntax(!value);
            }

            return new UnaryExpressionSyntax(UnaryOperationKind.LogicalNot, condition);
        }
    }

    public static IEnumerable<BasicBlock> BreadthSearch(BasicBlock startNode)
    {
        var visited = new HashSet<BasicBlock>();
        var queue = new Queue<BasicBlock>();
        visited.Add(startNode);
        queue.Enqueue(startNode);
        while (queue.Count != 0)
        {
            var node = queue.Dequeue();
            yield return node;
            foreach (var nextNode in node.Outgoing.Select(x => x.To).Where(n => !visited.Contains(n)))
            {
                visited.Add(nextNode);
                queue.Enqueue(nextNode);
            }
        }
    }

    public override string ToString()
    {
        string Quote(string text)
        {
            return "\"" + text.TrimEnd()
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace(Environment.NewLine, "\\l") + "\"";
        }

        var writer = new StringWriter();
        writer.WriteLine("digraph G {");

        var blockIds = new Dictionary<BasicBlock, string>();

        for (var i = 0; i < Blocks.Count; i++)
        {
            var id = $"N{i}";
            blockIds.Add(Blocks[i], id);
        }

        foreach (var block in Blocks)
        {
            var id = blockIds[block];
            var label = Quote(block.ToString());
            writer.WriteLine($"    {id} [label = {label}, shape = box]");
        }

        foreach (var branch in Branches)
        {
            var fromId = blockIds[branch.From];
            var toId = blockIds[branch.To];
            var label = Quote(branch.ToString());
            writer.WriteLine($"    {fromId} -> {toId} [label = {label}]");
        }

        writer.WriteLine("}");
        return writer.ToString();
    }
}
