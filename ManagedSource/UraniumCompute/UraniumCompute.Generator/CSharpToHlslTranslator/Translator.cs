using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UraniumCompute.Generator.CSharpToHlslTranslator;

internal partial class Translator
{
    private readonly StringWriter codeWriter = new();
    
    internal TranslatedMethod Translate(CompiledMethod method)
    {
        codeWriter.WriteLine("[numthreads(1, 1, 1)]");
        codeWriter.Write($"{method.Declaration.ReturnType} ");
        codeWriter.Write($"{method.Name}");
        codeWriter.WriteLine($"({TranslateParameters(method.Declaration.ParameterList)})");
        codeWriter.WriteLine(Translate(method.Body));
        
        var hlslCode = codeWriter.ToString();
        return new TranslatedMethod(method.Name, hlslCode);
    }

    private string TranslateParameters(ParameterListSyntax parameterList)
    {
        return "uint3 globalInvocationID : SV_DispatchThreadID";
    }
}
