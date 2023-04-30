using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UraniumCompute.Generator.Translator;

internal static partial class CSharpToHlslTranslator
{
    private static string Translate(CSharpSyntaxNode node)
    {
        return node switch
        {
            VariableDeclarationSyntax declaration => Translate(declaration),
            VariableDeclaratorSyntax declarator => Translate(declarator),
            ElseClauseSyntax elseClause => Translate(elseClause, 0),
            // ArgumentSyntax argument => Translate(argument),
            // ConversionOperatorDeclarationSyntax declaration => Translate(declaration),
            // IndexerDeclarationSyntax declaration => Translate(declaration),
            _ => throw new NotImplementedException($"{node.Kind()} is not implemented")
        };
    }

    private static string Translate(ElseClauseSyntax elseClause, int nestingLevel)
    {
        return Translate(elseClause.Statement, nestingLevel);
    }

    private static string Translate(VariableDeclarationSyntax declaration)
    {
        var translate = $"{declaration.Type} {string.Join(", ", declaration.Variables.Select(Translate))}";
        return translate;
    }

    private static string Translate(VariableDeclaratorSyntax declarator)
    {
        var value = declarator.Initializer is null ? "" : $" = {Translate(declarator.Initializer.Value)}";
        var translate = $"{declarator.Identifier}{value}";
        return translate;
    }
}
