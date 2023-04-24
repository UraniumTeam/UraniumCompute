using UraniumCompute.Generator;

namespace GeneratorSample;

internal class Program
{
    [CompileKernel]
    public int SomeMethod1()
    {
        return 5+5;
    }
    
    public string SomeMethod2()
    {
        return "00";
    }

    [CompileKernel]
    public static string SomeMethod3()
    {
        return "0";
    }

    public static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
    }
}
