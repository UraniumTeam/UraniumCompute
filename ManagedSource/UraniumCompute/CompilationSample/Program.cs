using UraniumCompute.Compiler.Decompiling;

namespace CompilationSample;

internal static class Program
{
    private static int SimpleMethod()
    {
        var index = 9;
        int index2, index3;
        index2 = (index + index) * index;
        index3 = index + index * index;
        return 100000;
    }
    
    private static int SimpleMethod(int a)
    {
        var index = 9;
        int index2, index3;
        index2 = (index + index) * index;
        index3 = index + index * index;
        return 100000;
    }

    private static void Main()
    {
        Func<int> simpleMethod = SimpleMethod;
        var compilation = MethodCompilation.Create(simpleMethod, nameof(SimpleMethod));
        var result = compilation.Compile();
        Console.WriteLine(result.HlslCode);
    }
}
