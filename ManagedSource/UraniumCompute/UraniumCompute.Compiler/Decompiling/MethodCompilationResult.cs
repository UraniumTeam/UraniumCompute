namespace UraniumCompute.Compiler.Decompiling;

public readonly struct MethodCompilationResult
{
    public string? HlslCode { get; }
    public IReadOnlyList<Diagnostic> Diagnostics { get; }

    internal MethodCompilationResult(string? hlslCode, IReadOnlyList<Diagnostic> diagnostics)
    {
        HlslCode = hlslCode;
        Diagnostics = diagnostics;
    }
}
