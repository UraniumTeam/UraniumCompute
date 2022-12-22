using System.Diagnostics;
using Mono.Cecil;
using Mono.Cecil.Cil;
using UraniumCompute.Common.Math;
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
    public List<StructDeclarationSyntax> Structs { get; }
    
    public IReadOnlyList<ParameterDeclarationSyntax> Parameters =>
        Function?.Parameters ?? new List<ParameterDeclarationSyntax>();

    public FunctionDeclarationSyntax? Function { get; init; }

    private readonly DisassemblyResult disassemblyResult;
    private readonly Stack<ExpressionSyntax> stack = new();
    private readonly Instruction[] instructions;
    private readonly Action<MethodReference> userFunctionCallback;
    private int argumentIndexOffset => disassemblyResult.HasThis ? 1 : 0;

    private readonly Dictionary<int, LabelStatementSyntax> labels = new();

    private int instructionIndex;
    private Instruction? Current => instructionIndex < instructions.Length ? instructions[instructionIndex] : null;

    private SyntaxTree(Action<MethodReference> userFunctionCallback, DisassemblyResult disassemblyResult,
        Instruction[] instructions, int instructionIndex, string methodName, IReadOnlyList<TypeReference> variableTypes,
        IEnumerable<StructDeclarationSyntax> structs)
    {
        this.userFunctionCallback = userFunctionCallback;
        this.disassemblyResult = disassemblyResult;
        this.instructions = instructions;
        this.instructionIndex = instructionIndex;
        Structs = structs.ToList();
        MethodName = methodName;
        VariableTypes = variableTypes;
    }

    private SyntaxTree(Action<MethodReference> userFunctionCallback, KernelAttribute? attribute, string methodName,
        DisassemblyResult dr, IEnumerable<StructDeclarationSyntax> structs)
    {
        this.userFunctionCallback = userFunctionCallback;
        MethodName = methodName;
        disassemblyResult = dr;
        VariableTypes = dr.Variables.Select(v => v.VariableType).ToArray();
        instructions = dr.Instructions.ToArray();
        Function = new FunctionDeclarationSyntax(attribute, methodName,
            TypeResolver.CreateType(dr.ReturnType, UserTypeCallback));
        Structs = structs.ToList();
    }

    public SyntaxTree WithStatements(IEnumerable<StatementSyntax> statements)
    {
        return new SyntaxTree(userFunctionCallback, disassemblyResult, instructions, instructionIndex, MethodName,
            VariableTypes, Structs)
        {
            Function = Function?.WithStatements(statements)
        };
    }

    internal static SyntaxTree Create(Action<MethodReference> userFunctionCallback, KernelAttribute? attribute,
        DisassemblyResult dr, string methodName = "main")
    {
        return new SyntaxTree(userFunctionCallback, attribute, methodName, dr, Enumerable.Empty<StructDeclarationSyntax>());
    }

    internal void Compile()
    {
        FindLabels();
        ParseParameters();

        for (var i = 0; i < VariableTypes.Count; i++)
        {
            AddStatement(new VariableDeclarationStatementSyntax(
                TypeResolver.CreateType(VariableTypes[i], UserTypeCallback),
                $"V_{i}"));
        }

        while (Current is not null)
        {
            if (labels.ContainsKey(Current.Offset))
            {
                AddStatement(labels[Current.Offset]);
            }

            ParseStatement();
        }

        Debug.Assert(!stack.Any());
    }

    public void GenerateCode(ICodeGenerator generator)
    {
        foreach (var s in Structs)
        {
            generator.EmitStruct(s);
        }

        generator.EmitFunction(Function!);
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

    private void NextInstruction()
    {
        instructionIndex++;
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
            var parameterType = TypeResolver.CreateType(parameter.ParameterType, UserTypeCallback);
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
        switch (Current!.OpCode.Code)
        {
            case Code.Ldind_I:
            case Code.Ldind_I1:
            case Code.Ldind_I2:
            case Code.Ldind_I4:
            case Code.Ldind_I8:
            case Code.Ldind_R4:
            case Code.Ldind_R8:
            case Code.Ldind_Ref:
            case Code.Ldind_U1:
            case Code.Ldind_U2:
            case Code.Ldind_U4:
                NextInstruction();
                return;
        }

        switch (Current!.OpCode.Code)
        {
            case Code.Nop:
                NextInstruction();
                return;
            case Code.Ret:
                AddStatement(new ReturnStatementSyntax(stack.Any() ? stack.Pop() : null));
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
            ParseConversionExpression,
            ParseVariableExpression,
            ParseAssignmentVariableExpression,
            ParseAssignmentIndirectExpression,
            ParseAssignmentArgumentExpression,
            ParseAssignmentFieldExpression,
            ParseInitObjectExpression,
            ParseArgumentExpression,
            ParseNewobjExpression,
            ParseCallExpression,
            ParseBranchExpression,
            ParseFieldExpression
        };

        return expressionParsers.Any(parser => parser());
    }

    private LiteralExpressionSyntax? CreateLiteral()
    {
        return Current!.OpCode.Code switch
        {
            Code.Ldnull => throw new InvalidOperationException("null is not supported by GPU"),
            Code.Ldc_I4_M1 => new LiteralExpressionSyntax(-1),
            Code.Ldc_I4_0 => new LiteralExpressionSyntax(0),
            Code.Ldc_I4_1 => new LiteralExpressionSyntax(1),
            Code.Ldc_I4_2 => new LiteralExpressionSyntax(2),
            Code.Ldc_I4_3 => new LiteralExpressionSyntax(3),
            Code.Ldc_I4_4 => new LiteralExpressionSyntax(4),
            Code.Ldc_I4_5 => new LiteralExpressionSyntax(5),
            Code.Ldc_I4_6 => new LiteralExpressionSyntax(6),
            Code.Ldc_I4_7 => new LiteralExpressionSyntax(7),
            Code.Ldc_I4_8 => new LiteralExpressionSyntax(8),
            Code.Ldc_I4_S => new LiteralExpressionSyntax((sbyte)Current!.Operand & 0xff),
            Code.Ldc_I4 => new LiteralExpressionSyntax((int)Current!.Operand),
            Code.Ldc_R4 => new LiteralExpressionSyntax((float)Current!.Operand),
            Code.Ldc_R8 => new LiteralExpressionSyntax((double)Current!.Operand),
            Code.Ldc_I8 => throw new InvalidOperationException("64-bit ints are not supported by GPU"),
            _ => null
        };
    }

    private bool ParseLiteralExpression()
    {
        var literal = CreateLiteral();
        if (literal is null)
        {
            return false;
        }

        stack.Push(literal);
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

    private bool ParseConversionExpression()
    {
        switch (Current!.OpCode.Code)
        {
            case Code.Conv_U1:
            case Code.Conv_Ovf_U1:
            case Code.Conv_Ovf_U1_Un:
            case Code.Conv_I1:
            case Code.Conv_Ovf_I1:
            case Code.Conv_Ovf_I1_Un:
                throw new Exception("8-bit ints are not supported by GPU");
            case Code.Conv_U2:
            case Code.Conv_Ovf_U2:
            case Code.Conv_Ovf_U2_Un:
            case Code.Conv_I2:
            case Code.Conv_Ovf_I2:
            case Code.Conv_Ovf_I2_Un:
                throw new Exception("16-bit ints are not supported by GPU");
            case Code.Conv_I4:
            case Code.Conv_Ovf_I4:
            case Code.Conv_Ovf_I4_Un:
            case Code.Conv_I:
            case Code.Conv_Ovf_I:
            case Code.Conv_Ovf_I_Un:
                stack.Push(new ConversionExpression(stack.Pop(), TypeResolver.CreateType<int>()));
                break;
            case Code.Conv_I8:
            case Code.Conv_Ovf_I8:
            case Code.Conv_Ovf_I8_Un:
            case Code.Conv_U8:
            case Code.Conv_Ovf_U8:
            case Code.Conv_Ovf_U8_Un:
                throw new Exception("64-bit ints are not supported by GPU");
            case Code.Conv_U:
            case Code.Conv_Ovf_U:
            case Code.Conv_Ovf_U_Un:
            case Code.Conv_U4:
            case Code.Conv_Ovf_U4:
            case Code.Conv_Ovf_U4_Un:
                stack.Push(new ConversionExpression(stack.Pop(), TypeResolver.CreateType<uint>()));
                break;
            case Code.Conv_R_Un:
            case Code.Conv_R4:
                stack.Push(new ConversionExpression(stack.Pop(), TypeResolver.CreateType<float>()));
                break;
            case Code.Conv_R8:
                stack.Push(new ConversionExpression(stack.Pop(), TypeResolver.CreateType<double>()));
                break;
            default:
                return false;
        }

        NextInstruction();
        return true;
    }

    private int GetVariableIndex(bool store)
    {
        if (store)
        {
            switch (Current!.OpCode.Code)
            {
                case Code.Stloc_0: return 0;
                case Code.Stloc_1: return 1;
                case Code.Stloc_2: return 2;
                case Code.Stloc_3: return 3;
                case Code.Stloc:
                case Code.Stloc_S:
                    return ((VariableDefinition)Current!.Operand).Index;
                default:
                    return -1;
            }
        }

        switch (Current!.OpCode.Code)
        {
            case Code.Ldloc_0: return 0;
            case Code.Ldloc_1: return 1;
            case Code.Ldloc_2: return 2;
            case Code.Ldloc_3: return 3;
            case Code.Ldloc:
            case Code.Ldloc_S:
            case Code.Ldloca:
            case Code.Ldloca_S:
                return ((VariableDefinition)Current!.Operand).Index;
            default:
                return -1;
        }
    }

    private bool ParseAssignmentVariableExpression()
    {
        var variableIndex = GetVariableIndex(true);
        if (variableIndex == -1)
        {
            return false;
        }

        var variableType = TypeResolver.CreateType(VariableTypes[variableIndex], UserTypeCallback);
        AddStatement(new AssignmentStatementSyntax(
            stack.Pop(),
            new VariableExpressionSyntax(variableIndex, variableType)));
        NextInstruction();
        return true;
    }

    private bool ParseVariableExpression()
    {
        var variableIndex = GetVariableIndex(false);
        if (variableIndex == -1)
        {
            return false;
        }

        var variableType = TypeResolver.CreateType(VariableTypes[variableIndex], UserTypeCallback);
        stack.Push(new VariableExpressionSyntax(variableIndex, variableType));
        NextInstruction();
        return true;
    }

    private bool ParseAssignmentIndirectExpression()
    {
        switch (Current!.OpCode.Code)
        {
            case Code.Stind_I:
            case Code.Stind_I1:
            case Code.Stind_I2:
            case Code.Stind_I4:
            case Code.Stind_I8:
            case Code.Stind_R4:
            case Code.Stind_R8:
            case Code.Stind_Ref:
                break;
            default:
                return false;
        }

        AddStatement(new AssignmentStatementSyntax(stack.Pop(), stack.Pop()));
        NextInstruction();
        return true;
    }

    private bool ParseAssignmentArgumentExpression()
    {
        var index = GetArgumentIndex(true);
        if (index == -1)
        {
            return false;
        }

        var name = Parameters[index].Name;
        var type = Parameters[index].ParameterType;
        AddStatement(new AssignmentStatementSyntax(
            stack.Pop(),
            new ArgumentExpressionSyntax(name, type)));

        NextInstruction();
        return true;
    }

    private bool ParseAssignmentFieldExpression()
    {
        if (Current!.OpCode.Code != Code.Stfld)
        {
            return false;
        }

        var expression = stack.Pop();
        var field = CreateFieldExpression();
        AddStatement(new AssignmentStatementSyntax(expression, field));
        NextInstruction();
        return true;
    }

    private bool ParseInitObjectExpression()
    {
        if (Current!.OpCode.Code != Code.Initobj)
        {
            return false;
        }

        // TODO: default initialize the objects here
        stack.Pop();
        NextInstruction();
        return true;
    }

    private bool ParseArgumentExpression()
    {
        var index = GetArgumentIndex(false);
        if (index == -1)
        {
            return false;
        }

        var name = Parameters[index].Name;
        var type = Parameters[index].ParameterType;
        stack.Push(new ArgumentExpressionSyntax(name, type));

        NextInstruction();
        return true;
    }

    private int GetArgumentIndex(bool store)
    {
        switch (Current!.OpCode.Code)
        {
            case Code.Starg:
            case Code.Starg_S:
                return Current!.Operand is ParameterDefinition x && store ? x.Index : -1;
            case Code.Ldarg:
            case Code.Ldarga:
            case Code.Ldarg_S:
            case Code.Ldarga_S:
                return Current!.Operand is ParameterDefinition p && !store ? p.Index : -1;
            case Code.Ldarg_0: return store ? -1 : 0 - argumentIndexOffset;
            case Code.Ldarg_1: return store ? -1 : 1 - argumentIndexOffset;
            case Code.Ldarg_2: return store ? -1 : 2 - argumentIndexOffset;
            case Code.Ldarg_3: return store ? -1 : 3 - argumentIndexOffset;
            default:
                return -1;
        }
    }

    private bool ParseNewobjExpression()
    {
        if (Current!.OpCode.Code != Code.Newobj)
        {
            return false;
        }

        return ParseGeneralCallExpression((MethodReference)Current.Operand);
    }

    private bool ParseCallExpression()
    {
        if (Current!.OpCode.Code != Code.Call)
        {
            return false;
        }

        var callParsers = new List<Func<MethodReference, bool>>
        {
            ParseSystemCallExpression,
            ParseIntrinsicCallExpression,
            ParseConstructorCallExpression,
            ParseOperatorCallExpression,
            ParseGeneralCallExpression
        };

        return callParsers.Any(parser => parser((MethodReference)Current.Operand));
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
                stack.Push(new ArgumentExpressionSyntax("globalInvocationID", TypeResolver.CreateType<Vector3Uint>()));
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

    private bool ParseConstructorCallExpression(MethodReference methodReference)
    {
        if (!methodReference.Resolve().IsConstructor)
        {
            return false;
        }

        var functionSymbol = FunctionResolver.Resolve(methodReference, userFunctionCallback, UserTypeCallback);
        var arguments = functionSymbol.ArgumentTypes.Select(_ => stack.Pop()).Reverse();

        AddStatement(
            new AssignmentStatementSyntax(
                new CallExpressionSyntax(functionSymbol, arguments),
                stack.Pop()));
        NextInstruction();
        return true;
    }

    private bool ParseOperatorCallExpression(MethodReference methodReference)
    {
        var kind = BinaryExpressionSyntax.GetOperationKind(methodReference.Name);
        if (kind == BinaryOperationKind.None)
        {
            return false;
        }

        stack.Push(new BinaryExpressionSyntax(kind, stack.Pop(), stack.Pop()));
        NextInstruction();
        return true;
    }

    private bool ParseGeneralCallExpression(MethodReference methodReference)
    {
        var functionSymbol = FunctionResolver.Resolve(methodReference, userFunctionCallback, UserTypeCallback);
        var arguments = functionSymbol.ArgumentTypes.Select(_ => stack.Pop()).Reverse();
        stack.Push(new CallExpressionSyntax(functionSymbol, arguments));
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
                var comparison = opCode is Code.Brtrue or Code.Brtrue_S
                    ? stack.Pop()
                    : new UnaryExpressionSyntax(UnaryOperationKind.LogicalNot, stack.Pop());
                AddStatement(new ConditionalGotoStatementSyntax(comparison, ((Instruction)Current!.Operand).Offset));
                break;
            case Code.Bgt:
            case Code.Bgt_S:
            case Code.Bgt_Un:
            case Code.Bgt_Un_S:
                AddStatement(
                    new ConditionalGotoStatementSyntax(
                        new BinaryExpressionSyntax(BinaryOperationKind.Gt, stack.Pop(), stack.Pop()), 
                        ((Instruction)Current!.Operand).Offset));
                break;
            case Code.Blt:
            case Code.Blt_S:
            case Code.Blt_Un:
            case Code.Blt_Un_S:
                AddStatement(
                    new ConditionalGotoStatementSyntax(
                        new BinaryExpressionSyntax(BinaryOperationKind.Lt, stack.Pop(), stack.Pop()), 
                        ((Instruction)Current!.Operand).Offset));
                break;
            case Code.Bge:
            case Code.Bge_S:
            case Code.Bge_Un:
            case Code.Bge_Un_S:
                AddStatement(
                    new ConditionalGotoStatementSyntax(
                        new BinaryExpressionSyntax(BinaryOperationKind.Ge, stack.Pop(), stack.Pop()), 
                        ((Instruction)Current!.Operand).Offset));
                break;
            case Code.Ble:
            case Code.Ble_S:
            case Code.Ble_Un:
            case Code.Ble_Un_S:
                AddStatement(
                    new ConditionalGotoStatementSyntax(
                        new BinaryExpressionSyntax(BinaryOperationKind.Le, stack.Pop(), stack.Pop()), 
                        ((Instruction)Current!.Operand).Offset));
                break;
            default:
                return false;
        }

        NextInstruction();
        return true;
    }

    private bool ParseFieldExpression()
    {
        if (Current!.OpCode.Code != Code.Ldfld && Current!.OpCode.Code != Code.Ldflda)
        {
            return false;
        }

        stack.Push(CreateFieldExpression());
        NextInstruction();
        return true;
    }

    private ExpressionSyntax CreateFieldExpression()
    {
        var field = (FieldReference)Current!.Operand!;
        return new PropertyExpressionSyntax(stack.Pop(), field);
    }

    private void AddStatement(StatementSyntax statement)
    {
        Function!.Block.Statements.Add(statement);
    }

    private void UserTypeCallback(TypeReference typeReference)
    {
        Structs.Add(new StructDeclarationSyntax(StructTypeSymbol.CreateUserType(typeReference, UserTypeCallback)));
    }
}
