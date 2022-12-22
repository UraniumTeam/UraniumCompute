using UraniumCompute.Compiler.Decompiling;
using UraniumCompute.Compiler.Syntax;

namespace UraniumCompute.Compiler.CodeGen;

internal sealed class HlslCodeGenerator : ICodeGenerator
{
    public int IndentSize { get; }
    public TextWriter Output { get; private set; }

    public HlslCodeGenerator(TextWriter output, int indentSize)
    {
        Output = output;
        IndentSize = indentSize;
    }

    public string CreateForwardDeclaration(FunctionDeclarationSyntax syntax)
    {
        var temp = Output;
        Output = new StringWriter();
        EmitFunctionDeclarationImpl(syntax);
        Output.WriteLine(';');
        var result = Output.ToString();
        Output = temp;
        return result!;
    }

    public string CreateForwardDeclaration(StructDeclarationSyntax syntax)
    {
        return $"struct {syntax.StructType.FullName};{Environment.NewLine}";
    }

    public void EmitFunction(FunctionDeclarationSyntax syntax)
    {
        EmitFunctionDeclarationImpl(syntax);
        Output.WriteLine();
        EmitStatement(syntax.Block, 0);
    }

    public void EmitStruct(StructDeclarationSyntax syntax)
    {
        Output.WriteLine($"struct {syntax.StructType.FullName}");
        Output.WriteLine("{");
        foreach (var field in syntax.StructType.Fields)
        {
            WriteIndent(1);
            Output.WriteLine($"{field.FieldType} {field.Name};");
        }

        Output.WriteLine("};");
    }

    private void EmitConstants(ParameterDeclarationSyntax syntax)
    {
        Output.WriteLine($"cbuffer Constants : register(b{syntax.BindingIndex})");
        Output.WriteLine("{");
        WriteIndent(1);
        Emit(syntax, false);
        Output.WriteLine("};");
    }

    private void EmitFunctionDeclarationImpl(FunctionDeclarationSyntax syntax)
    {
        if (syntax.KernelAttribute is not null)
        {
            foreach (var parameter in syntax.Parameters)
            {
                if (parameter.ParameterType is GenericBufferTypeSymbol)
                {
                    Emit(parameter, true);
                    continue;
                }

                EmitConstants(parameter);
            }

            Emit(syntax.KernelAttribute, 0);
        }

        var parameters = syntax.IsEntryPoint
            ? "uint3 globalInvocationID : SV_DispatchThreadID"
            : string.Join(", ", syntax.Parameters.Select(x => x.ToStringWithType()));
        Output.Write($"{syntax.ReturnType} {syntax.FunctionName}({parameters})");
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
            case ConversionExpression syntax:
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
        Output.WriteLine($"{syntax.VariableType} {syntax.Name};");
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

    private void Emit(ConversionExpression syntax)
    {
        Output.Write("((");
        Output.Write(syntax.ExpressionType);
        Output.Write(")");
        EmitExpression(syntax.ConvertedExpression);
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
        Output.Write($"{syntax.CalledFunction.FullName}(");
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

    // ReSharper disable once UnusedParameter.Local
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

    private void Emit(ParameterDeclarationSyntax syntax, bool register)
    {
        Output.Write($"{syntax.ToStringWithType()}");

        if (register)
        {
            Output.Write($" : register(u{syntax.BindingIndex})");
        }

        Output.WriteLine(";");
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
        if (syntax.Expression is not null)
        {
            EmitExpression(syntax.Expression);
        }

        Output.WriteLine(';');
    }

    private void Emit(UnaryExpressionSyntax syntax)
    {
        Output.Write('(');
        Output.Write(UnaryExpressionSyntax.GetOperationString(syntax.Kind));
        EmitExpression(syntax.Expression);
        Output.Write(')');
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
