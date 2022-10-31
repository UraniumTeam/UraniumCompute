using Mono.Cecil;
using Mono.Cecil.Cil;
using UraniumCompute.Compiler.Disassembling;

namespace UraniumCompute.Compiler.Syntax;

public class SyntaxTree
{
    private readonly DisassemblyResult disassemblyResult;
    private readonly Stack<SyntaxNode> stack = new();

    internal IReadOnlyList<TypeReference> VariableTypes { get; }

    private SyntaxTree(DisassemblyResult dr)
    {
        disassemblyResult = dr;
        VariableTypes = dr.Variables.Select(v => v.VariableType).ToArray();
    }

    internal static SyntaxTree Create(DisassemblyResult dr)
    {
        return new SyntaxTree(dr);
    }
    
    internal void Compile()
    {
        var instructions = disassemblyResult.Instructions;
        var parameters = disassemblyResult.Parameters;
        
        
        var statements = new List<StatementSyntax>();
        Console.WriteLine(disassemblyResult.Name);
        Console.WriteLine(disassemblyResult.ReturnType);
        foreach (var variable in disassemblyResult.Variables)
        {
            Console.WriteLine(variable.VariableType);
        }
        foreach (var instruction in instructions)
        {
            Console.WriteLine(instruction);
        }
    }

    private StatementSyntax ParseStatement()
    {
        // stack.
        return new ExpressionStatementSyntax(ParseExpression());
    }
    
    private ExpressionSyntax ParseExpression()
    {
        // stack.Push(new BinaryOperationSyntax(BinaryOperationKind.Addition, stack.Pop(), stack.Pop()));
        throw new NotImplementedException();
        // return new StatementSyntax();
    }
}