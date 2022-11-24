using UraniumCompute.Compiler.Decompiling;
using UraniumCompute.Compiler.Disassembling;
using UraniumCompute.Compiler.Syntax;

namespace UraniumCompute.Compiler.CodeGen;

internal sealed class HlslCodeGenerator : ICodeGenerator
{
    public TextWriter Output { get; }
    public int IndentSize { get; }

    public HlslCodeGenerator(TextWriter output, int indentSize)
    {
        Output = output;
        IndentSize = indentSize;
    }

    public void EmitFunctionDeclaration(FunctionDeclarationSyntax syntax)
    {
        if (syntax.KernelAttribute is not null)
        {
            foreach (var parameter in syntax.Parameters)
            {
                Emit(parameter);
            }
            Emit(syntax.KernelAttribute, 0);
        }

        var parameters = syntax.IsEntryPoint
            ? "uint3 globalInvocationID : SV_DispatchThreadID"
            : string.Join(", ", syntax.Parameters);
        Output.WriteLine($"{TypeResolver.ConvertType(syntax.ReturnType)} {syntax.FunctionName}({parameters})");
        EmitStatement(syntax.Block, 0);
    }

    private void EmitStatement(StatementSyntax statement, int indent)
    {
        switch (statement)
        {
            case AssignmentStatementSyntax syntax:
                Emit(syntax, indent);
                break;
            case BlockStatementSyntax syntax:
                Emit(syntax, indent);
                break;
            case ConditionalGotoStatementSyntax syntax:
                Emit(syntax, indent);
                break;
            case GotoStatementSyntax syntax:
                Emit(syntax, indent);
                break;
            case IfStatementSyntax syntax:
                Emit(syntax, indent);
                break;
            case WhileStatementSyntax syntax:
                Emit(syntax, indent);
                break;
            case BreakStatementSyntax syntax:
                Emit(syntax, indent);
                break;
            case LabelStatementSyntax syntax:
                Emit(syntax, indent);
                break;
            case ReturnStatementSyntax syntax:
                Emit(syntax, indent);
                break;
            case VariableDeclarationStatementSyntax syntax:
                Emit(syntax, indent);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(statement));
        }
    }

    private void EmitExpression(ExpressionSyntax expression)
    {
        switch (expression)
        {
            case ArgumentExpressionSyntax syntax:
                Emit(syntax);
                break;
            case BinaryExpressionSyntax syntax:
                Emit(syntax);
                break;
            case CallExpressionSyntax syntax:
                Emit(syntax);
                break;
            case IndexerExpressionSyntax syntax:
                Emit(syntax);
                break;
            case LiteralExpressionSyntax syntax:
                Emit(syntax);
                break;
            case PropertyExpressionSyntax syntax:
                Emit(syntax);
                break;
            case UnaryExpressionSyntax syntax:
                Emit(syntax);
                break;
            case VariableExpressionSyntax syntax:
                Emit(syntax);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(expression));
        }
    }

    private void Emit(VariableDeclarationStatementSyntax syntax, int indent)
    {
        WriteIndent(indent);
        Output.WriteLine($"{TypeResolver.ConvertType(syntax.VariableType)} {syntax.Name};");
    }

    private void Emit(KernelAttribute kernelAttribute, int indent)
    {
        WriteIndent(indent);
        Output.WriteLine($"[numthreads({kernelAttribute.X}, {kernelAttribute.Y}, {kernelAttribute.Z})]");
    }

    private void Emit(ArgumentExpressionSyntax syntax)
    {
        Output.Write(syntax.Name);
    }

    private void Emit(AssignmentStatementSyntax syntax, int indent)
    {
        WriteIndent(indent);
        EmitExpression(syntax.Left);
        Output.Write(" = ");
        EmitExpression(syntax.Right);
        Output.WriteLine(';');
    }

    private void Emit(BinaryExpressionSyntax syntax)
    {
        Output.Write("(");
        EmitExpression(syntax.Left);
        Output.Write($" {BinaryExpressionSyntax.GetOperationString(syntax.Kind)} ");
        EmitExpression(syntax.Right);
        Output.Write(")");
    }

    private void Emit(BlockStatementSyntax syntax, int indent)
    {
        WriteIndent(indent);
        Output.WriteLine("{");
        foreach (var statement in syntax.Statements)
        {
            EmitStatement(statement, indent + 1);
        }

        WriteIndent(indent);
        Output.WriteLine("}");
    }

    private void Emit(CallExpressionSyntax syntax)
    {
        Output.WriteLine($"{syntax.FunctionName}(");
        foreach (var argument in syntax.Arguments)
        {
            EmitExpression(argument);
            if (argument != syntax.Arguments.Last())
            {
                Output.Write(", ");
            }
        }

        Output.Write(")");
    }

    private void Emit(ConditionalGotoStatementSyntax syntax, int indent)
    {
        WriteIndent(indent);
        Output.Write("if (");
        EmitExpression(syntax.Condition);
        Output.WriteLine($") {{ goto lbl_{syntax.Offset}; }}");
    }

    private void Emit(GotoStatementSyntax syntax, int indent)
    {
        WriteIndent(indent);
        Output.WriteLine($"goto lbl_{syntax.Offset};");
    }

    private void Emit(IfStatementSyntax syntax, int indent)
    {
        WriteIndent(indent);
        Output.Write("if (");
        EmitExpression(syntax.Condition);
        Output.WriteLine(")");
        Emit(syntax.ThenBlock, indent);
        if (syntax.ElseClause is not null)
        {
            WriteIndent(indent);
            Output.WriteLine("else");
            Emit(syntax.ElseClause.Block, indent);
        }
    }

    private void Emit(WhileStatementSyntax syntax, int indent)
    {
        WriteIndent(indent);
        Output.Write("while (");
        EmitExpression(syntax.Condition);
        Output.WriteLine(")");
        Emit(syntax.Block, indent);
    }

    private void Emit(IndexerExpressionSyntax syntax)
    {
        EmitExpression(syntax.IndexedExpression);
        Output.Write('[');
        EmitExpression(syntax.Index);
        Output.Write(']');
    }

    private void Emit(BreakStatementSyntax syntax, int indent)
    {
        WriteIndent(indent);
        Output.WriteLine("break;");
    }

    private void Emit(LabelStatementSyntax syntax, int indent)
    {
        WriteIndent(indent - 1);
        Output.WriteLine($"lbl_{syntax.Offset}:");
    }

    private void Emit(LiteralExpressionSyntax syntax)
    {
        Output.Write(syntax);
    }

    private void Emit(ParameterDeclarationSyntax syntax)
    {
        Output.WriteLine($"{syntax.ToStringWithType()} : register(u{syntax.BindingIndex});");
    }

    private void Emit(PropertyExpressionSyntax syntax)
    {
        EmitExpression(syntax.Instance);
        Output.Write($".{syntax.PropertyName}");
    }

    private void Emit(ReturnStatementSyntax syntax, int indent)
    {
        WriteIndent(indent);
        Output.Write("return ");
        EmitExpression(syntax.Expression);
        Output.WriteLine(';');
    }

    private void Emit(UnaryExpressionSyntax syntax)
    {
        Output.Write(UnaryExpressionSyntax.GetOperationString(syntax.Kind));
        EmitExpression(syntax.Expression);
    }

    private void Emit(VariableExpressionSyntax syntax)
    {
        Output.Write($"V_{syntax.Index}");
    }

    private void WriteIndent(int indent)
    {
        Output.Write(new string(' ', IndentSize * indent));
    }
}
