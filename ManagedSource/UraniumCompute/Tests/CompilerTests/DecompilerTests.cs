using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using UraniumCompute.Compiler.Decompiling;
using UraniumCompute.Compiler.InterimStructs;

namespace CompilerTests;

public partial class DecompilerTests
{
    [Test]
    public void CompilesLiterals()
    {
        AssertFunc(() => 100000, """
            [numthreads(1, 1, 1)]
            int main(uint3 globalInvocationID : SV_DispatchThreadID)
            {
                return 100000;
            }
            """);
    }

    [Test]
    public void CompilesKernelAttribute()
    {
        AssertFunc([Kernel(X = 3, Y = 4, Z = 5)]() => 100000, """
            [numthreads(3, 4, 5)]
            int main(uint3 globalInvocationID : SV_DispatchThreadID)
            {
                return 100000;
            }
            """);
    }

    [Test]
    public void CompilesExplicitReturn()
    {
        AssertFunc(() => { return 100000; }, """
            [numthreads(1, 1, 1)]
            int main(uint3 globalInvocationID : SV_DispatchThreadID)
            {
                int V_0;
                V_0 = 100000;
                return V_0;
            }
            """);
    }

    [Test]
    public void CompilesBinaryExpressions()
    {
        AssertFunc((Func<int>)(() =>
        {
            var index = 9;
            return index + index * index;
        }), """
            [numthreads(1, 1, 1)]
            int main(uint3 globalInvocationID : SV_DispatchThreadID)
            {
                int V_0;
                int V_1;
                V_0 = 9;
                V_1 = (V_0 + (V_0 * V_0));
                return V_1;
            }
            """);
    }

    [Test]
    public void CompilesBinaryExpressions_WithParentheses()
    {
        AssertFunc((Func<int>)(() =>
        {
            var index = 9;
            return (index + index) * index;
        }), """
            [numthreads(1, 1, 1)]
            int main(uint3 globalInvocationID : SV_DispatchThreadID)
            {
                int V_0;
                int V_1;
                V_0 = 9;
                V_1 = ((V_0 + V_0) * V_0);
                return V_1;
            }
            """);
    }

    [Test]
    public void CompilesBufferParameter_ReadOnly()
    {
        AssertFunc((Span<int> a) => a[10], """
            RWStructuredBuffer<int> a : register(u0);
            [numthreads(1, 1, 1)]
            int main(uint3 globalInvocationID : SV_DispatchThreadID)
            {
                return a[10];
            }
            """);
    }

    [Test]
    public void CompilesBufferParameter_WriteOnly()
    {
        AssertFunc((Span<int> a) =>
        {
            a[0] = 6;
            return 0;
        }, """
            RWStructuredBuffer<int> a : register(u0);
            [numthreads(1, 1, 1)]
            int main(uint3 globalInvocationID : SV_DispatchThreadID)
            {
                int V_0;
                a[0] = 6;
                V_0 = 0;
                return V_0;
            }
            """);
    }

    [Test]
    public void CompilesIntFloatConversion()
    {
        AssertFunc((Span<float> values) =>
        {
            var index = GpuIntrinsic.GetGlobalInvocationId().X;
            values[(int)index] = index;
        }, """
            RWStructuredBuffer<float> values : register(u0);
            [numthreads(1, 1, 1)]
            void main(uint3 globalInvocationID : SV_DispatchThreadID)
            {
                uint V_0;
                V_0 = globalInvocationID.x;
                values[V_0] = ((float)((float)V_0));
                return ;
            }
            """);
    }

    private static void AssertFunc(Delegate func, [StringSyntax("Cpp")] string expectedHlslCode)
    {
        var actualCode = MethodCompilation.Compile(func);
        Assert.That(NormalizeCode(actualCode), Is.EqualTo(NormalizeCode(expectedHlslCode)));
    }

    private static string NormalizeCode(string code)
    {
        return Regex.Replace(code, @"\s+", " ").Trim();
    }
}
