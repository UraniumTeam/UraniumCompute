using System.Diagnostics;
using System.Reflection;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using UraniumCompute.Compiler.CodeGen;
using UraniumCompute.Compiler.Decompiling;
using UraniumCompute.Compiler.Disassembling;
using UraniumCompute.Compiler.InterimStructs;
using UraniumCompute.Compiler.Rewriting;

namespace UraniumCompute.Compiler.Syntax;

internal class SyntaxTree
{
    public string MethodName { get; }
    public IReadOnlyList<TypeReference> VariableTypes { get; }
    public FunctionDeclarationSyntax? Function { get; init; }
    public IReadOnlyList<ParameterDeclarationSyntax> Parameters => Function?.Parameters ?? new List<ParameterDeclarationSyntax>();

    private readonly MethodInfo method;
    private readonly DisassemblyResult disassemblyResult;
    private readonly Stack<ExpressionSyntax> stack = new();
    private readonly Instruction[] instructions;

    private readonly Dictionary<int, LabelStatementSyntax> labels = new();

    private int instructionIndex;
    private Instruction? Current => instructionIndex < instructions.Length ? instructions[instructionIndex] : null;

    public SyntaxTree WithStatements(IEnumerable<StatementSyntax> statements)
    {
        return new SyntaxTree(method, disassemblyResult, instructions, instructionIndex, MethodName, VariableTypes)
        {
            Function = Function?.WithStatements(statements)
        };
    }

    private void NextInstruction()
    {
        instructionIndex++;
    }

    private SyntaxTree(MethodInfo method, DisassemblyResult disassemblyResult, Instruction[] instructions, int instructionIndex,
        string methodName,
        IReadOnlyList<TypeReference> variableTypes)
    {
        this.method = method;
        this.disassemblyResult = disassemblyResult;
        this.instructions = instructions;
        this.instructionIndex = instructionIndex;
        MethodName = methodName;
        VariableTypes = variableTypes;
    }

    private SyntaxTree(MethodInfo method, string methodName, DisassemblyResult dr)
    {
        this.method = method;
        MethodName = methodName;
        disassemblyResult = dr;
        VariableTypes = dr.Variables.Select(v => v.VariableType).ToArray();
        instructions = dr.Instructions.ToArray();
        var attribute = method.GetCustomAttribute<KernelAttribute>() ?? new KernelAttribute();
        Function = new FunctionDeclarationSyntax(attribute, methodName, dr.ReturnType, new List<ParameterDeclarationSyntax>(),
            new BlockStatementSyntax());
    }

    internal static SyntaxTree Create(MethodInfo method, DisassemblyResult dr, string methodName = "main")
    {
        return new SyntaxTree(method, methodName, dr);
    }

    internal void Compile()
    {
        FindLabels();
        ParseParameters();

        for (var i = 0; i < VariableTypes.Count; i++)
        {
            AddStatement(new VariableDeclarationStatementSyntax(VariableTypes[i], $"V_{i}"));
        }

        while (Current is not null)
        {
            if (labels.ContainsKey(Current.Offset))
            {
                AddStatement(labels[Current.Offset]);
            }

            ParseStatement();
        }
    }

    private void FindLabels()
    {
        foreach (var instruction in instructions)
        {
            if (instruction.Operand is Instruction operand)
            {
                labels[operand.Offset] = new LabelStatementSyntax(operand.Offset);
            }
        }
    }

    private void ParseParameters()
    {
        for (var i = 0; i < disassemblyResult.Parameters.Count; ++i)
        {
            var parameter = disassemblyResult.Parameters[i];
            var parameterType = Disassembler.ConvertType(parameter.ParameterType);
            Function!.Parameters.Add(new ParameterDeclarationSyntax(parameterType, parameter.Name, i));
        }
    }

    private void ParseStatement()
    {
        if (ParseExpression())
        {
            return;
        }

        // For now load indirect instructions do nothing in our case
        if (Current!.OpCode.Name.StartsWith("ldind"))
        {
            NextInstruction();
            return;
        }

        switch (Current!.OpCode.Code)
        {
            case Code.Nop:
                NextInstruction();
                return;
            case Code.Ret:
                if (stack.Any())
                {
                    AddStatement(new ReturnStatementSyntax(stack.Pop()));
                }

                NextInstruction();
                return;
            case Code.Dup:
                stack.Push(stack.Peek());
                NextInstruction();
                return;
            default:
                Debug.Fail($"Unknown instruction: {Current}");
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
            ParseCallExpression,
            ParseBranchExpression
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
            case Code.Ldloca:
            case Code.Ldloca_S:
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

        AddStatement(new AssignmentStatementSyntax(
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

        AddStatement(new AssignmentStatementSyntax(stack.Pop(), stack.Pop()));
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

        var callParsers = new List<Func<MethodReference, bool>>
        {
            ParseSystemCallExpression,
            ParseIntrinsicCallExpression,
            ParseIndex3DCallExpression
        };

        return callParsers.Any(parser => parser((MethodReference)Current.Operand));
    }

    private bool ParseIndex3DCallExpression(MethodReference methodReference)
    {
        // TODO: get rid of this method, parse general struct declarations and member functions instead
        if (methodReference.DeclaringType.FullName != typeof(Index3D).FullName)
        {
            return false;
        }

        switch (methodReference.Name)
        {
            case "get_X":
                stack.Push(new PropertyExpressionSyntax(stack.Pop(), "x"));
                break;
            case "get_Y":
                stack.Push(new PropertyExpressionSyntax(stack.Pop(), "y"));
                break;
            case "get_Z":
                stack.Push(new PropertyExpressionSyntax(stack.Pop(), "z"));
                break;
            default:
                throw new InvalidOperationException($"Unknown instruction: {Current}");
        }

        NextInstruction();
        return true;
    }

    private bool ParseIntrinsicCallExpression(MethodReference methodReference)
    {
        if (methodReference.DeclaringType.FullName != typeof(GpuIntrinsic).FullName)
        {
            return false;
        }

        switch (methodReference.Name)
        {
            case nameof(GpuIntrinsic.GetGlobalInvocationId):
                stack.Push(new ArgumentExpressionSyntax("globalInvocationID"));
                break;
            default:
                throw new InvalidOperationException($"Unknown instruction: {Current}");
        }

        NextInstruction();
        return true;
    }

    private bool ParseSystemCallExpression(MethodReference methodReference)
    {
        if (methodReference.DeclaringType.Namespace != typeof(int).Namespace)
        {
            return false;
        }

        var callParsers = new List<Func<MethodReference, bool>>
        {
            ParseSpanCallExpression
        };

        return callParsers.Any(parser => parser(methodReference));
    }

    private bool ParseSpanCallExpression(MethodReference methodReference)
    {
        if (methodReference.DeclaringType.Name != typeof(Span<>).Name)
        {
            return false;
        }

        switch (methodReference.Name)
        {
            case "get_Item":
                stack.Push(new IndexerExpressionSyntax(stack.Pop(), stack.Pop()));
                break;
            default:
                throw new InvalidOperationException($"Unknown instruction: {Current}");
        }

        NextInstruction();
        return true;
    }

    private bool ParseBranchExpression()
    {
        var opCode = Current!.OpCode.Code;
        switch (opCode)
        {
            case Code.Br:
            case Code.Br_S:
                AddStatement(new GotoStatementSyntax(((Instruction)Current!.Operand).Offset));
                break;
            case Code.Brfalse:
            case Code.Brfalse_S:
            case Code.Brtrue:
            case Code.Brtrue_S:
                var comparison = new BinaryExpressionSyntax(BinaryOperationKind.Eq,
                    new LiteralExpressionSyntax(opCode is Code.Brtrue or Code.Brtrue_S),
                    stack.Pop());
                AddStatement(new ConditionalGotoStatementSyntax(comparison, ((Instruction)Current!.Operand).Offset));
                break;
            default:
                return false;
        }

        NextInstruction();
        return true;
    }

    private void AddStatement(StatementSyntax statement)
    {
        Function!.Block.Statements.Add(statement);
    }

    public void GenerateCode(ICodeGenerator generator)
    {
        generator.EmitFunctionDeclaration(Function!);
    }

    public override string ToString()
    {
        return Function?.ToString() ?? "<Empty>";
    }

    public static ISyntaxTreeRewriter[] GetStandardPasses()
    {
        return new ISyntaxTreeRewriter[]
        {
            new BranchResolver()
        };
    }

    public SyntaxTree Rewrite(params ISyntaxTreeRewriter[] passes)
    {
        return passes.Aggregate(this, (current, rewriter) => rewriter.Rewrite(current));
    }
}
