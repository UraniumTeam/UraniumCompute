using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UraniumCompute.Generator.Translator;

internal static partial class CSharpToHlslTranslator
{
    private static string Translate(CSharpSyntaxNode node)
    {
        throw new NotImplementedException($"{node.Kind()} is not implemented");
    }

    private static string Translate(ElseClauseSyntax elseClause, int nestingLevel)
    {
        return Translate(elseClause.Statement, nestingLevel);
    }

    private static string Translate(VariableDeclarationSyntax declaration)
    {
        if (declaration.Type.IsVar)
            throw new NotSupportedException($"{declaration.Type} is not supported by HLSL");
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
