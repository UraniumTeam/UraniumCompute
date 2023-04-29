using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UraniumCompute.Generator.Translator;

internal static partial class CSharpToHlslTranslator
{
    private static string Translate(ExpressionSyntax? expression)
    {
        return expression switch
        {
            null => "",
            LiteralExpressionSyntax literalExpression => Translate(literalExpression),
            BinaryExpressionSyntax binaryExpression => Translate(binaryExpression),
            PostfixUnaryExpressionSyntax postfixUnaryExpression => Translate(postfixUnaryExpression),
            PrefixUnaryExpressionSyntax prefixUnaryExpression => Translate(prefixUnaryExpression),
            _ => throw new NotImplementedException($"{expression.Kind()} is not implemented")
        };
    }

    private static string Translate(LiteralExpressionSyntax expression)
    {
        return expression.ToString();
    }

    private static string Translate(BinaryExpressionSyntax expression)
    {
        return $"({Translate(expression.Left)} " +
               $"{GetBinaryOperator(expression.Kind())} " +
               $"{Translate(expression.Right)})";
    }

    private static string GetBinaryOperator(SyntaxKind kind)
    {
        return kind switch
        {
            SyntaxKind.AddExpression => "+",
            SyntaxKind.SubtractExpression => "-",
            SyntaxKind.MultiplyExpression => "*",
            SyntaxKind.DivideExpression => "/",
            SyntaxKind.ModuloExpression => "%",
            SyntaxKind.LeftShiftExpression => "<<",
            SyntaxKind.RightShiftExpression => ">>",
            SyntaxKind.LogicalOrExpression => "||",
            SyntaxKind.LogicalAndExpression => "&&",
            SyntaxKind.BitwiseOrExpression => "|",
            SyntaxKind.BitwiseAndExpression => "&",
            SyntaxKind.ExclusiveOrExpression => "^",
            SyntaxKind.EqualsExpression => "=",
            SyntaxKind.NotEqualsExpression => "!=",
            SyntaxKind.LessThanExpression => "<",
            SyntaxKind.LessThanOrEqualExpression => "<=",
            SyntaxKind.GreaterThanExpression => ">",
            SyntaxKind.GreaterThanOrEqualExpression => ">=",
            _ => throw new NotSupportedException($"{kind} is not supported by HLSL")
        };
    }

    private static string Translate(PrefixUnaryExpressionSyntax expression)
    {
        return $"{GetUnaryOperator(expression.Kind())}{Translate(expression.Operand)}";
    }

    private static string Translate(PostfixUnaryExpressionSyntax expression)
    {
        return $"{Translate(expression.Operand)}{GetUnaryOperator(expression.Kind())}";
    }

    private static string GetUnaryOperator(SyntaxKind kind)
    {
        return kind switch
        {
            SyntaxKind.UnaryPlusExpression => "+",
            SyntaxKind.UnaryMinusExpression => "-",
            SyntaxKind.BitwiseNotExpression => "~",
            SyntaxKind.LogicalNotExpression => "!",
            SyntaxKind.PreIncrementExpression => "++",
            SyntaxKind.PreDecrementExpression => "--",
            SyntaxKind.PostIncrementExpression => "++",
            SyntaxKind.PostDecrementExpression => "--",
            _ => throw new NotSupportedException($"{kind} is not supported by HLSL")
        };
    }
}
