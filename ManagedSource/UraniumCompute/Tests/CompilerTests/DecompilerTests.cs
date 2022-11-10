using Mono.Cecil;
using UraniumCompute.Compiler.Decompiling;

namespace CompilerTests;

public class Tests
{
    [Test]
    public void Test1()
    {
        var expectedResult = "int <Test1>b__0_0() { return 100000; }";
        AssertFunc(() => 100000, expectedResult);
    }

    [Test]
    public void Test2()
    {
        var expectedResult = "int <Test2>b__1_0() { int V_0; V_0 = 100000; return V_0; }";
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
        var expectedResult = $"int {func.Method.Name}() {{ " +
                             $"int V_0; int V_1; V_0 = 9; V_1 = ((V_0 + V_0) * V_0); return V_1; }}";
        AssertFunc(func, expectedResult);
    }

    private static void AssertFunc(Delegate func, string expectedHlslCode)
    {
        var compilation = MethodCompilation.Create(func);
        Assert.That(compilation.Compile().HlslCode, Is.EqualTo(expectedHlslCode));
    }
}
