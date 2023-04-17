using UraniumCompute.Generator;

namespace GeneratorSample;

internal class Program
{
    [CompileKernel]
    public int SomeMethod1()
    {
        return 0;
    }

    [CompileKernel]
    public string SomeMethod2()
    {
        return "00";
    }

    [CompileKernel]
    public string SomeMethod3()
    {
        return "0";
    }

    public static void Main(string[] args)
    {
        TempGenerated.TempClass.Write();
        //TempGenerated2
        Console.WriteLine("He, World!");
    }
}
