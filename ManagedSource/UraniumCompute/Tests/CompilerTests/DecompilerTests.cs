using Mono.Cecil;
using UraniumCompute.Compiler.Decompiling;

namespace CompilerTests;

public class Tests
{
    [Test]
    public void Test1()
    {
        var expectedResult = "[numthreads(1, 1, 1)] " +
                             "int main() " +
                             "{ return 100000; }";
        AssertFunc(() => 100000, expectedResult);
    }

    [Test]
    public void Test2()
    {
        var expectedResult = "[numthreads(1, 1, 1)] " +
                             "int main() " +
                             "{ int V_0; V_0 = 100000; return V_0; }";
        AssertFunc(() => { return 100000; }, expectedResult);
    }

    [Test]
    public void Test3()
    {
        var func = () =>
        {
            var index = 9;
            return (index + index) * index;
        };
        var expectedResult = "[numthreads(1, 1, 1)] " +
                             "int main() " +
                             "{ int V_0; int V_1; " +
                             "V_0 = 9; " +
                             "V_1 = ((V_0 + V_0) * V_0); " +
                             "return V_1; }";
        AssertFunc(func, expectedResult);
    }
    
    [Test]
    public void Test4()
    {
        var func = (Span<int> a) =>
        {
            return a[10];
        };
        var expectedResult = "RWStructuredBuffer<int> a : register(u0); " +
                             "[numthreads(1, 1, 1)] " +
                             "int main() { " +
                             "int V_0; " +
                             "V_0 = a[10]; " +
                             "return V_0; }";
        AssertFunc(func, expectedResult);
    }
    
    [Test]
    public void Test5()
    {
        var func = (Span<int> a) =>
        {
            a[0] = 6;
            return 0;
        };
        var expectedResult = "RWStructuredBuffer<int> a : register(u0); " +
                             "[numthreads(1, 1, 1)] " +
                             "int main() { " +
                             "int V_0; " +
                             "a[0] = 6; " +
                             "V_0 = 0; " +
                             "return V_0; }";
        AssertFunc(func, expectedResult);
    }

    private static void AssertFunc(Delegate func, string expectedHlslCode)
    {
        var compilation = MethodCompilation.Create(func);
        Assert.That(compilation.Compile().HlslCode, Is.EqualTo(expectedHlslCode));
    }
}
