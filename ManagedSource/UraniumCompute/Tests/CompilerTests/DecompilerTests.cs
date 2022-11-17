using Mono.Cecil;
using UraniumCompute.Compiler.Decompiling;
using UraniumCompute.Compiler.InterimStructs;

namespace CompilerTests;

public class Tests
{
    [Test]
    public void Test1()
    {
        var expectedResult = "[numthreads(1, 1, 1)] " +
                             "int main(uint3 globalInvocationID : SV_DispatchThreadID) " +
                             "{ return 100000; }";
        AssertFunc(() => 100000, expectedResult);
    }

    [Test]
    public void Test2()
    {
        var expectedResult = "[numthreads(1, 1, 1)] " +
                             "int main(uint3 globalInvocationID : SV_DispatchThreadID) " +
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
                             "int main(uint3 globalInvocationID : SV_DispatchThreadID) " +
                             "{ int V_0; int V_1; " +
                             "V_0 = 9; " +
                             "V_1 = ((V_0 + V_0) * V_0); " +
                             "return V_1; }";
        AssertFunc(func, expectedResult);
    }

    [Test]
    public void Test4()
    {
        var func = (Span<int> a) => { return a[10]; };
        var expectedResult = "RWStructuredBuffer<int> a : register(u0); " +
                             "[numthreads(1, 1, 1)] " +
                             "int main(uint3 globalInvocationID : SV_DispatchThreadID) { " +
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
                             "int main(uint3 globalInvocationID : SV_DispatchThreadID) { " +
                             "int V_0; " +
                             "a[0] = 6; " +
                             "V_0 = 0; " +
                             "return V_0; }";
        AssertFunc(func, expectedResult);
    }

    [Test]
    public void Test6()
    {
        var func = (Span<int> a) =>
        {
            var index = GpuIntrinsic.GetGlobalInvocationId();
            a[index.X] *= 2;
        };
        var expectedResult = "RWStructuredBuffer<int> a : register(u0); " +
                             "[numthreads(1, 1, 1)] " +
                             "void main(uint3 globalInvocationID : SV_DispatchThreadID) { " +
                             "uint3 V_0; " +
                             "V_0 = globalInvocationID; " +
                             "a[V_0.x] = (a[V_0.x] * 2); }";
        AssertFunc(func, expectedResult);
    }

    [Test]
    public void Test7()
    {
        var func = () =>
        {
            var index = GpuIntrinsic.GetGlobalInvocationId();
            var x = index.X;
            var y = index.Y;
            var z = index.Z;
            return x + y;
        };
        var expectedResult = "[numthreads(1, 1, 1)] " +
                             "int main(uint3 globalInvocationID : SV_DispatchThreadID) { " +
                             "uint3 V_0; " +
                             "int V_1; " +
                             "int V_2; " +
                             "int V_3; " +
                             "int V_4; " +
                             "V_0 = globalInvocationID; " +
                             "V_1 = V_0.x; " +
                             "V_2 = V_0.y; " +
                             "V_3 = V_0.z; " +
                             "V_4 = (V_1 + V_2); " +
                             "return V_4; }";
        AssertFunc(func, expectedResult);
    }

    private static void AssertFunc(Delegate func, string expectedHlslCode)
    {
        var compilation = MethodCompilation.Create(func);
        var actualCode = compilation.Compile().HlslCode;
        Assert.That(actualCode, Is.EqualTo(expectedHlslCode));
    }
}
