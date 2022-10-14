using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UraniumCompute.Compiler.Decompiling;

namespace CompilationSample;

[StructLayout(LayoutKind.Sequential)]
public readonly struct Index3D
{
    public int X { get; }
    public int Y { get; }
    public int Z { get; }

    public Index3D(int x, int y, int z)
    {
        X = x;
        Y = y;
        Z = z;
    }
}

public static class GpuIntrinsic
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static Index3D GetGlobalInvocationId()
    {
        // placeholder, used in kernel compilation
        return default;
    }
}

internal static class Program
{
    private static int Add(Span<int> a, Span<int> b)
    {
        var index = GpuIntrinsic.GetGlobalInvocationId().X;
        return a[index] + b[index];
    }
    
    private static void Main()
    {
        var compilation = MethodCompilation.Create(typeof(Program), nameof(Add));
        var result = compilation.Compile();
        Console.WriteLine(result.HlslCode);
    }
}
