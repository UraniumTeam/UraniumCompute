using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UraniumCompute.Generator.CSharpToHlslTranslator;

internal partial class Translator
{
    private string Translate(StatementSyntax statement)
    {
        return statement switch
        {
            BlockSyntax block => Translate(block),
            ReturnStatementSyntax returnStatement => Translate(returnStatement),
            _ => throw new NotImplementedException($"{statement.Kind()} is not implemented")
        };
    }

    private string Translate(BlockSyntax block)
    {
        var sb = new StringWriter();
        sb.WriteLine("{");
        foreach (var statement in block.Statements)
        {
            sb.WriteLine($"    {Translate(statement)}");
        }
        sb.WriteLine("}");

        return sb.ToString();
    }

    private string Translate(ReturnStatementSyntax statement)
    {
        return $"return {Translate(statement.Expression)};";
    }
}
