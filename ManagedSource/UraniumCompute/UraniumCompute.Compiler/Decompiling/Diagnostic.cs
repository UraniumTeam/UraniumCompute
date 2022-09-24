namespace UraniumCompute.Compiler.Decompiling;

public sealed class Diagnostic
{
    public string Message { get; }
    // public TestSpan TextLocation { get; } // can be retrieved from method debug info later...

    internal Diagnostic(string message)
    {
        Message = message;
    }

    public override string ToString()
    {
        return $"Method compilation error: {Message}";
    }
}
