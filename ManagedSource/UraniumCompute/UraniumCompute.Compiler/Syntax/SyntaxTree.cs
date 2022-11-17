﻿using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using UraniumCompute.Compiler.Disassembling;

namespace UraniumCompute.Compiler.Syntax;

internal class SyntaxTree
{
    internal string MethodName { get; }
    internal IReadOnlyList<TypeReference> VariableTypes { get; }
    internal DisassemblyResult DisassemblyResult { get; }

    private readonly Stack<ExpressionSyntax> stack = new();
    private readonly List<StatementSyntax> statements = new();
    private readonly List<ParameterExpressionSyntax> parameters = new();
    private readonly Instruction[] instructions;

    private int instructionIndex;
    private Instruction? Current => instructionIndex < instructions.Length ? instructions[instructionIndex] : null;

    private void NextInstruction()
    {
        instructionIndex++;
    }

    private SyntaxTree(string methodName, DisassemblyResult dr)
    {
        MethodName = methodName;
        DisassemblyResult = dr;
        VariableTypes = dr.Variables.Select(v => v.VariableType).ToArray();
        instructions = dr.Instructions.ToArray();
    }

    internal static SyntaxTree Create(DisassemblyResult dr, string methodName = "main")
    {
        return new SyntaxTree(methodName, dr);
    }

    internal void Compile()
    {
        ParseParameters();

        while (Current is not null)
        {
            ParseStatement();
        }
    }

    private void ParseParameters()
    {
        foreach (var parameter in DisassemblyResult.Parameters)
        {
            parameters.Add(
                new ParameterExpressionSyntax(Disassembler.ConvertType(parameter.ParameterType), parameter.Name));
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
                if (stack.Count != 0)
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
            ParseAssignmentVarExpression,
            ParseVariableExpression,
            ParseAssignmentArgExpression,
            ParseArgumentExpression,
            ParseCallExpression
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
            Code.Ldc_I4_S => (sbyte)Current!.Operand & 0xff,
            Code.Ldc_I4 => (int)Current!.Operand,
            Code.Ldc_R4 => (float)Current!.Operand,
            Code.Ldc_R8 => (double)Current!.Operand,
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

    private int GetVariableIndex()
    {
        switch (Current!.OpCode.Code)
        {
            case Code.Stloc_0:
            case Code.Ldloc_0: return 0;
            case Code.Stloc_1:
            case Code.Ldloc_1: return 1;
            case Code.Stloc_2:
            case Code.Ldloc_2: return 2;
            case Code.Stloc_3:
            case Code.Ldloc_3: return 3;
            case Code.Stloc:
            case Code.Stloc_S:
            case Code.Ldloc:
            case Code.Ldloc_S:
                return ((VariableDefinition)Current!.Operand).Index;
        }

        throw new InvalidOperationException($"Invalid instruction: {Current}");
    }

    private bool ParseAssignmentVarExpression()
    {
        if (!Current!.OpCode.Name.StartsWith("stloc"))
        {
            return false;
        }

        statements.Add(new AssignmentExpressionSyntax(
            stack.Pop(),
            new VariableExpressionSyntax(GetVariableIndex())));
        NextInstruction();
        return true;
    }

    private bool ParseVariableExpression()
    {
        if (!Current!.OpCode.Name.StartsWith("ldloc"))
        {
            return false;
        }

        stack.Push(new VariableExpressionSyntax(GetVariableIndex()));
        NextInstruction();
        return true;
    }

    private bool ParseAssignmentArgExpression()
    {
        if (!Current!.OpCode.Name.StartsWith("stind"))
        {
            return false;
        }

        statements.Add(new AssignmentExpressionSyntax(stack.Pop(), stack.Pop()));
        NextInstruction();
        return true;
    }

    private bool ParseArgumentExpression()
    {
        if (!Current!.OpCode.Name.StartsWith("ldarga"))
        {
            return false;
        }

        stack.Push(new ArgumentExpressionSyntax($"{Current.Operand}"));
        NextInstruction();
        return true;
    }

    private bool ParseCallExpression()
    {
        if (Current!.OpCode.Name != "call")
        {
            return false;
        }

        var operand = (MethodReference)Current.Operand;
        switch (operand.Name)
        {
            case "get_Item":
                stack.Push( new IndexerExpressionSyntax(stack.Pop(), stack.Pop()));
                break;
            default:
                throw new InvalidOperationException($"Unknown instruction: {Current}");
        }

        NextInstruction();
        return true;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        for (var i = 0; i < parameters.Count; ++i)
        {
            sb.Append($"{parameters[i].ToStringWithType()} : register(u{i}); ");
        }

        sb.Append($"[numthreads(1, 1, 1)] ");
        sb.Append($"{Disassembler.ConvertType(DisassemblyResult.ReturnType)} {MethodName}() {{ ");

        for (var i = 0; i < VariableTypes.Count; ++i)
        {
            sb.Append($"{Disassembler.ConvertType(VariableTypes[i])} V_{i}; ");
        }

        foreach (var statement in statements)
        {
            sb.Append($"{statement} ");
        }

        sb.Append('}');
        return sb.ToString();
    }
}
