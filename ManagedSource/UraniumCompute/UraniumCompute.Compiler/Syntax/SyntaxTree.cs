using Mono.Cecil;
using Mono.Cecil.Cil;
using UraniumCompute.Compiler.Disassembling;

namespace UraniumCompute.Compiler.Syntax;

public class SyntaxTree
{
    private readonly DisassemblyResult dr;
    private readonly Stack<SyntaxNode> stack = new();

    internal IReadOnlyList<TypeReference> VariableTypes { get; }

    private SyntaxTree(DisassemblyResult dr)
    {
        this.dr = dr;
        VariableTypes = dr.Variables.Select(v => v.VariableType).ToArray();
    }

    internal static SyntaxTree Create(DisassemblyResult dr)
    {
        return new SyntaxTree(dr);
    }

    internal void Compile()
    {
        var instructions = dr.Instructions;
        var parameters = dr.Parameters;
        var variables = dr.Variables;
        //var statements = new List<StatementSyntax>();
        Console.WriteLine(dr.Name);
        // Console.WriteLine(dr.ReturnType);
        // foreach (var variableType in VariableTypes)
        // {
        //     Console.WriteLine(variableType);
        // }
        foreach (var instruction in instructions)
        {
            Console.WriteLine(instruction);
            ParseStatement(instruction);
        }

        throw new NotImplementedException();
    }

    private void ParseStatement(Instruction statement)
    {
        if (ExpressionSyntax.Expressions.Contains(statement.OpCode.Name))
            ParseExpression(statement);
        // stack.Push(new ExpressionStatementSyntax(ParseExpression(statement)));
    }

    private void ParseExpression(Instruction expression)
    {
        if (BinaryExpressionSyntax.BinaryCilOperations.TryGetValue(expression.OpCode.Name, out var kind))
            stack.Push(new BinaryExpressionSyntax(
                kind,
                (LiteralExpressionSyntax)stack.Pop(),
                (LiteralExpressionSyntax)stack.Pop()));
        //throw new NotImplementedException();
        // return new StatementSyntax();
    }
}