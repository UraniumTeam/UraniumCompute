using System.Text;

namespace UraniumCompute.Compiler.Syntax;

internal sealed class BlockStatementSyntax : StatementSyntax
{
    public List<StatementSyntax> Statements { get; }

    public BlockStatementSyntax()
    {
        Statements = new List<StatementSyntax>();
    }

    public BlockStatementSyntax(IEnumerable<StatementSyntax> statements)
    {
        Statements = statements.ToList();
    }

    public BlockStatementSyntax(StatementSyntax statement)
    {
        Statements = new List<StatementSyntax> { statement };
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append("{ ");
        foreach (var statement in Statements)
        {
            sb.Append($"{statement} ");
        }

        sb.Append("} ");

        return sb.ToString();
    }
}
