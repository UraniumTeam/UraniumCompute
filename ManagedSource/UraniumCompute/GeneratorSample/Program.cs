using UraniumCompute.Generator;

namespace GeneratorSample;

internal class Program
{
    [CompileKernel]
    public static int SomeMethod1()
    {
        // don't use "var"
        int m = 9, j = 1;
        for (int i = j = 4; i < 5 + 5; ++i, j--)
        {
            if (i > 6)
                m += i;
            else
                m -= j;
            if (i == 7)
                break;
        }
        while (j < 0)
        {
            j += 2;
        }
        return j;
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
        // build the project to have access to MethodsTranslatedToHlsl
        Console.WriteLine($"{MethodsTranslatedToHlsl.SomeMethod1.Code}");
    }
}
