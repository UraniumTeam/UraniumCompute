using UraniumCompute.Compiler.Decompiling;

namespace CompilerTests;

public class Tests
{
    private int Test1Func()
    {
        return 100000;
    }

    [Test]
    public void Test1()
    {
        var compilation = MethodCompilation.Create(typeof(Tests), nameof(Test1Func));
        var result = compilation.Compile().HlslCode;
        Assert.That(
            compilation.Compile().HlslCode!,
            Is.EqualTo(
@"int Test1Func() {
int V_0;
V_0 = 100000;
return V_0;
}"));
    }
}
