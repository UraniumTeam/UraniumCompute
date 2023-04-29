using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UraniumCompute.Generator.Translator;

internal static partial class CSharpToHlslTranslator
{
    internal static TranslatedMethod Translate(CompiledMethod method)
    {
        var codeWriter = new StringWriter();
        codeWriter.WriteLine("[numthreads(1, 1, 1)]");
        codeWriter.Write($"{method.Declaration.ReturnType} ");
        codeWriter.Write($"{method.Name}");
        codeWriter.WriteLine($"({TranslateParameters(method.Declaration.ParameterList)})");
        codeWriter.WriteLine(Translate(method.Body));
        
        var hlslCode = codeWriter.ToString();
        return new TranslatedMethod(method.Name, hlslCode);
    }

    private static string TranslateParameters(ParameterListSyntax parameterList)
    {
        return "uint3 globalInvocationID : SV_DispatchThreadID";
    }
}
