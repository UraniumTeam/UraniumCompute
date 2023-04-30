using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UraniumCompute.Generator.Translator;

internal static partial class CSharpToHlslTranslator
{
    private static string GetIndent(int nestingLevel) => new string(' ', 4 * nestingLevel);

    private static string Translate(StatementSyntax statement, int nestingLevel)
    {
        return statement switch
        {
            BlockSyntax block => Translate(block, nestingLevel),
            ReturnStatementSyntax returnStatement => Translate(returnStatement, nestingLevel),
            LocalDeclarationStatementSyntax declaration => Translate(declaration, nestingLevel),
            ExpressionStatementSyntax expressionStatement => Translate(expressionStatement, nestingLevel),
            ForStatementSyntax forStatement => Translate(forStatement, nestingLevel),
            IfStatementSyntax ifStatement => Translate(ifStatement, nestingLevel),
            // While
            // Break
            _ => throw new NotImplementedException($"{statement.Kind()} is not implemented")
        };
    }

    private static string Translate(IfStatementSyntax statement, int nestingLevel)
    {
        var writer = new StringWriter();
        writer.WriteLine($"{GetIndent(nestingLevel)}if ({statement.Condition})");
        writer.Write($"{Translate(statement.Statement, nestingLevel + 1)}");
        if (statement.Else is not null)
        {
            writer.WriteLine($"\r\n{GetIndent(nestingLevel)}else");
            writer.Write($"{Translate(statement.Else, nestingLevel + 1)}");
        }
        return writer.ToString();
    }

    private static string Translate(ForStatementSyntax statement, int nestingLevel)
    {
        var writer = new StringWriter();
        writer.Write($"{GetIndent(nestingLevel)}for (");
        if (statement.Declaration is not null)
            writer.Write(Translate(statement.Declaration));
        writer.Write("; ");
        if (statement.Condition is not null)
            writer.Write(Translate(statement.Condition));
        writer.Write("; ");
        if (statement.Incrementors.Count != 0)
            writer.Write(string.Join(", ", statement.Incrementors.Select(Translate)));
        writer.WriteLine(")");
        writer.Write($"{Translate(statement.Statement, nestingLevel)}");
        return writer.ToString();
    }

    private static string Translate(ExpressionStatementSyntax expressionStatement, int nestingLevel)
    {
        return $"{GetIndent(nestingLevel)}{Translate(expressionStatement.Expression)};";
    }

    private static string Translate(LocalDeclarationStatementSyntax statement, int nestingLevel)
    {
        return $"{GetIndent(nestingLevel)}{Translate(statement.Declaration)};";
    }

    private static string Translate(BlockSyntax block, int nestingLevel)
    {
        var writer = new StringWriter();
        writer.WriteLine($"{GetIndent(nestingLevel)}{{");
        foreach (var statement in block.Statements)
        {
            writer.WriteLine($"{Translate(statement, nestingLevel + 1)}");
        }
        writer.Write($"{GetIndent(nestingLevel)}}}");

        return writer.ToString();
    }

    private static string Translate(ReturnStatementSyntax statement, int nestingLevel)
    {
        return GetIndent(nestingLevel) +
               $"return {(statement.Expression is null ? "" : Translate(statement.Expression))};";
    }
}
