namespace UraniumCompute.Compiler.Decompiling;

public readonly struct MethodCompilationResult
{
    public string? Code { get; }
    public string? Declaration { get; }
    public IReadOnlyList<Diagnostic> Diagnostics { get; }

    internal MethodCompilationResult(string? code, string? declaration, IReadOnlyList<Diagnostic> diagnostics)
    {
        Code = code;
        Declaration = declaration;
        Diagnostics = diagnostics;
    }
}
