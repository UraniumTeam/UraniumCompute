using UraniumCompute.Compiler.InterimStructs;
using UraniumCompute.Compiler.Decompiling;

namespace CompilationSample;

internal static class Program
{
    private static int Add(Span<int> a, Span<int> b)
    {
        var index = GpuIntrinsic.GetGlobalInvocationId().X;
        // if (index < 0)
        // {
        //     int index2;
        //     index2 = index;
        // }
        // else if (index == 0)
        // {
        //     int index2;
        //     index2 = index;
        // }
        // else
        // {
        //     int index2;
        //     index2 = index;
        // }

        return a[index] + b[index];
    }

    private static void Main()
    {
        var compilation = MethodCompilation.Create(typeof(Program), nameof(Add));
        var result = compilation.Compile();
        Console.WriteLine(result.HlslCode);
    }
}