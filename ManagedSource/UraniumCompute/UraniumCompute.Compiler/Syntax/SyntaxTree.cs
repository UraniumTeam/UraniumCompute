using System.Diagnostics;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using UraniumCompute.Compiler.Disassembling;

namespace UraniumCompute.Compiler.Syntax;

public class SyntaxTree
{
    internal IReadOnlyList<TypeReference> VariableTypes { get; }
    internal DisassemblyResult DisassemblyResult { get; }

    private readonly Stack<ExpressionSyntax> stack = new();
    private readonly List<StatementSyntax> statements = new();
    private readonly Instruction[] instructions;

    private int instructionIndex;
    private Instruction? Current => instructionIndex < instructions.Length ? instructions[instructionIndex] : null;

    private void NextInstruction()
    {
        instructionIndex++;
    }

    private SyntaxTree(DisassemblyResult dr)
    {
        DisassemblyResult = dr;
        VariableTypes = dr.Variables.Select(v => v.VariableType).ToArray();
        instructions = dr.Instructions.ToArray();
    }

    internal static SyntaxTree Create(DisassemblyResult dr)
    {
        return new SyntaxTree(dr);
    }

    internal void Compile()
    {
        while (Current is not null)
        {
            ParseStatement();
        }
    }

    private void ParseStatement()
    {
        if (ParseExpression())
        {
            return;
        }

        switch (Current!.OpCode.Code)
        {
            case Code.Nop:
                NextInstruction();
                return;
            case Code.Ret:
                statements.Add(new ReturnStatementSyntax(stack.Pop()));
                NextInstruction();
                return;
            default:
                Console.WriteLine($"Warning: Unknown instruction skipped: {Current}");
                NextInstruction();
                break;
        }
    }

    private bool ParseExpression()
    {
        var expressionParsers = new List<Func<bool>>
        {
            ParseLiteralExpression,
            ParseBinaryExpression,
            ParseAssignmentExpression,
            ParseVariableExpression
        };

        return expressionParsers.Any(parser => parser());
    }

    private object? GetLiteralValue()
    {
        return Current!.OpCode.Code switch
        {
            Code.Ldnull => null,
            Code.Ldc_I4_M1 => null,
            Code.Ldc_I4_0 => 0,
            Code.Ldc_I4_1 => 1,
            Code.Ldc_I4_2 => 2,
            Code.Ldc_I4_3 => 3,
            Code.Ldc_I4_4 => 4,
            Code.Ldc_I4_5 => 5,
            Code.Ldc_I4_6 => 6,
            Code.Ldc_I4_7 => 7,
            Code.Ldc_I4_8 => 8,
            Code.Ldc_I4_S => Current!.Operand,
            Code.Ldc_I4 => Current!.Operand,
            Code.Ldc_R4 => Current!.Operand,
            Code.Ldc_R8 => Current!.Operand,
            Code.Ldc_I8 => throw new InvalidOperationException("64-bit ints are not supported by GPU"),
            _ => throw new Exception()
        };
    }

    private bool ParseLiteralExpression()
    {
        if (!Current!.OpCode.Name.StartsWith("ldc"))
        {
            return false;
        }

        stack.Push(new LiteralExpressionSyntax(GetLiteralValue()));
        NextInstruction();
        return true;
    }

    private bool ParseBinaryExpression()
    {
        var kind = BinaryExpressionSyntax.GetOperationKind(Current!.OpCode.Code);
        if (kind == BinaryOperationKind.None)
        {
            return false;
        }

        stack.Push(new BinaryExpressionSyntax(kind, stack.Pop(), stack.Pop()));
        NextInstruction();
        return true;
    }

    private bool ParseAssignmentExpression()
    {
        if (!Current!.OpCode.Name.StartsWith("stloc"))
        {
            return false;
        }

        switch (Current!.OpCode.Code)
        {
            case Code.Stloc_0:
            case Code.Stloc_1:
            case Code.Stloc_2:
            case Code.Stloc_3:
            {
                statements.Add(
                    new AssignmentExpressionSyntax(int.Parse(Current!.OpCode.Name[6..]), stack.Pop()));
                break;
            }
            case Code.Stloc:
            case Code.Stloc_S:
            {
                statements.Add(
                    new AssignmentExpressionSyntax(int.Parse(Current!.Operand.ToString()![2..]), stack.Pop()));
                break;
            }
        }

        NextInstruction();
        return true;
    }

    private bool ParseVariableExpression()
    {
        if (!Current!.OpCode.Name.StartsWith("ldloc"))
        {
            return false;
        }

        switch (Current!.OpCode.Code)
        {
            case Code.Ldloc_0:
            case Code.Ldloc_1:
            case Code.Ldloc_2:
            case Code.Ldloc_3:
            {
                stack.Push(new VariableExpressionSyntax($"V_{int.Parse(Current!.OpCode.Name[6..])}"));
                break;
            }
            case Code.Ldloc_S:
            {
                stack.Push(new VariableExpressionSyntax($"V_{int.Parse(Current!.Operand.ToString()![2..])}"));
                break;
            }
        }

        NextInstruction();
        return true;
    }

    public static string ConvertType(TypeReference tr)
    {
        // TODO: move somewhere + handle errors without exceptions (use Diagnostics)
        Debug.Assert(tr.IsPrimitive, "Non-primitive types aren't supported yet...");
        return tr.Name switch
        {
            "Void" => "void",
            nameof(Byte) or nameof(SByte) =>
                throw new InvalidOperationException("8-bit ints are not supported by GPU"),
            nameof(Int16) or nameof(UInt16) =>
                throw new InvalidOperationException("16-bit ints are not supported by GPU"),
            nameof(Int64) or nameof(UInt64) =>
                throw new InvalidOperationException("64-bit ints are not supported by GPU"),
            nameof(Int32) => "int",
            nameof(UInt32) => "uint",
            nameof(Single) => "float",
            nameof(Double) => "double",
            nameof(Boolean) => "bool",
            _ => throw new Exception($"Unknown type: {tr.Name}")
        };
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"{ConvertType(DisassemblyResult.ReturnType)} {DisassemblyResult.Name}() {{");

        for (var i = 0; i < VariableTypes.Count; ++i)
        {
            sb.AppendLine($"{ConvertType(VariableTypes[i])} V_{i};");
        }

        foreach (var statement in statements)
        {
            sb.AppendLine(statement.ToString());
        }

        sb.Append('}');
        return sb.ToString();
    }
}