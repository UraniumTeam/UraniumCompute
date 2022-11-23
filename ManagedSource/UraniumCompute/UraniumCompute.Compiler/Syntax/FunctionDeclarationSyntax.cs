using System.Text;
using Mono.Cecil;
using UraniumCompute.Compiler.Decompiling;
using UraniumCompute.Compiler.Disassembling;

namespace UraniumCompute.Compiler.Syntax;

internal sealed class FunctionDeclarationSyntax : SyntaxNode
{
    public KernelAttribute? KernelAttribute { get; }
    public string FunctionName { get; }
    public TypeReference ReturnType { get; }
    public List<ParameterDeclarationSyntax> Parameters { get; }
    public BlockStatementSyntax Block { get; }
    public bool IsEntryPoint => KernelAttribute is not null;

    public FunctionDeclarationSyntax(KernelAttribute? kernelAttribute, string functionName, TypeReference returnType,
        List<ParameterDeclarationSyntax> parameters, BlockStatementSyntax block)
    {
        KernelAttribute = kernelAttribute;
        FunctionName = functionName;
        ReturnType = returnType;
        Parameters = parameters;
        Block = block;
    }

    public FunctionDeclarationSyntax WithStatements(IEnumerable<StatementSyntax> statements)
    {
        return new FunctionDeclarationSyntax(KernelAttribute, FunctionName, ReturnType, Parameters.ToList(),
            new BlockStatementSyntax(statements));
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        if (KernelAttribute is not null)
        {
            sb.Append(KernelAttribute);
            sb.Append(' ');
        }

        var parameters = IsEntryPoint ? "uint3 globalInvocationID : SV_DispatchThreadID" : string.Join(", ", Parameters);
        sb.Append($"{Disassembler.ConvertType(ReturnType)} {FunctionName}({parameters}) {Block}");
        return sb.ToString();
    }
}
