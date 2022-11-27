using System.Text.RegularExpressions;
using UraniumCompute.Compiler.Decompiling;

namespace CompilerTests;

public partial class DecompilerTests
{
    [Test]
    public void CompilesLiterals()
    {
        var expectedResult = @"[numthreads(1, 1, 1)]
int main(uint3 globalInvocationID : SV_DispatchThreadID)
{
    return 100000;
}
";
        AssertFunc(() => 100000, expectedResult);
    }

    [Test]
    public void CompilesKernelAttribute()
    {
        var expectedResult = @"[numthreads(3, 4, 5)]
int main(uint3 globalInvocationID : SV_DispatchThreadID)
{
    return 100000;
}
";
        AssertFunc([Kernel(X = 3, Y = 4, Z = 5)]() => 100000, expectedResult);
    }

    [Test]
    public void CompilesExplicitReturn()
    {
        var expectedResult = @"[numthreads(1, 1, 1)]
int main(uint3 globalInvocationID : SV_DispatchThreadID)
{
    int V_0;
    V_0 = 100000;
    return V_0;
}
";
        AssertFunc(() => { return 100000; }, expectedResult);
    }

    [Test]
    public void CompilesBinaryExpressions()
    {
        var expectedResult = @"[numthreads(1, 1, 1)]
int main(uint3 globalInvocationID : SV_DispatchThreadID)
{
    int V_0;
    int V_1;
    V_0 = 9;
    V_1 = (V_0 + (V_0 * V_0));
    return V_1;
}
";
        AssertFunc((Func<int>)(() =>
        {
            var index = 9;
            return index + index * index;
        }), expectedResult);
    }

    [Test]
    public void CompilesBinaryExpressions_WithParentheses()
    {
        var expectedResult = @"[numthreads(1, 1, 1)]
int main(uint3 globalInvocationID : SV_DispatchThreadID)
{
    int V_0;
    int V_1;
    V_0 = 9;
    V_1 = ((V_0 + V_0) * V_0);
    return V_1;
}
";
        AssertFunc((Func<int>)(() =>
        {
            var index = 9;
            return (index + index) * index;
        }), expectedResult);
    }

    [Test]
    public void CompilesBufferParameter_ReadOnly()
    {
        var expectedResult = @"RWStructuredBuffer<int> a : register(u0);
[numthreads(1, 1, 1)]
int main(uint3 globalInvocationID : SV_DispatchThreadID)
{
    return a[10];
}
";
        AssertFunc((Span<int> a) => a[10], expectedResult);
    }

    [Test]
    public void CompilesBufferParameter_WriteOnly()
    {
        var expectedResult = @"RWStructuredBuffer<int> a : register(u0);
[numthreads(1, 1, 1)]
int main(uint3 globalInvocationID : SV_DispatchThreadID)
{
    int V_0;
    a[0] = 6;
    V_0 = 0;
    return V_0;
}
";
        AssertFunc((Span<int> a) =>
        {
            a[0] = 6;
            return 0;
        }, expectedResult);
    }

    private static void AssertFunc(Delegate func, string expectedHlslCode)
    {
        var actualCode = MethodCompilation.Compile(func);
        Assert.That(NormalizeCode(actualCode), Is.EqualTo(NormalizeCode(expectedHlslCode)));
    }

    private static string NormalizeCode(string code)
    {
        return Regex.Replace(code, @"\s+", " ").Trim();
    }
}
