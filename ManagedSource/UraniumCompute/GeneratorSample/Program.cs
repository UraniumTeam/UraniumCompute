using UraniumCompute.Generator;

namespace GeneratorSample;

internal class Program
{
    [CompileKernel]
    public static int SomeMethod1()
    {
        return 5 + 5;
    }

    public string SomeMethod2()
    {
        return "00";
    }

    // [CompileKernel]
    public string SomeMethod3()
    {
        return SomeMethod2();
    }

    public static void Main(string[] args)
    {
        Console.WriteLine($"{MethodsTranslatedToHlsl.SomeMethod1.Code}");
    }
}
