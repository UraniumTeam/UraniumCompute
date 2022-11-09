using UraniumCompute.Compiler.InterimStructs;
using UraniumCompute.Compiler.Decompiling;

namespace CompilationSample;

internal static class Program
{
    private static int Add(Span<int> a, Span<int> b)
    {
        // var index = GpuIntrinsic.GetGlobalInvocationId().X;
        var index = 9;
        var index2 = index - 7;
        var index3 = index - 7;
        var index4 = index - 7;
        var index5 = index - 7;
        var index6 = index - 7;
        var index7 = index - 7;
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
        var r = (index > index2) == false;
        
        return 100000;
    }

    private static void Main()
    {
        var compilation = MethodCompilation.Create(typeof(Program), nameof(Add));
        var result = compilation.Compile();
        Console.WriteLine(result.HlslCode);
    }
}
