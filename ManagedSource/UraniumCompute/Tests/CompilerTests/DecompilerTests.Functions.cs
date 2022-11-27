using UraniumCompute.Compiler.InterimStructs;

namespace CompilerTests;

public partial class DecompilerTests
{
    [Test]
    public void CompilesCos()
    {
        var expectedResult = @"RWStructuredBuffer<float> values : register(u0);
[numthreads(1, 1, 1)]
void main(uint3 globalInvocationID : SV_DispatchThreadID)
{
    int V_0;
    V_0 = globalInvocationID.x;
    values[V_0] = cos(values[V_0]);
}
";

        AssertFunc((Span<float> values) =>
        {
            var index = (int)GpuIntrinsic.GetGlobalInvocationId().X;
            values[index] = MathF.Cos(values[index]);
        }, expectedResult);
    }

    [Test]
    public void CompilesMinMax()
    {
        var expectedResult = @"RWStructuredBuffer<float> values : register(u0);
[numthreads(1, 1, 1)]
void main(uint3 globalInvocationID : SV_DispatchThreadID)
{
    int V_0;
    V_0 = globalInvocationID.x;
    values[V_0] = max(values[V_0], min(1, values[V_0]));
}";

        AssertFunc((Span<float> values) =>
        {
            var index = (int)GpuIntrinsic.GetGlobalInvocationId().X;
            values[index] = MathF.Max(values[index], MathF.Min(1, values[index]));
        }, expectedResult);
    }

    [Test]
    public void CompilesSinCos()
    {
        var expectedResult = @"RWStructuredBuffer<float> values : register(u0);
[numthreads(1, 1, 1)]
void main(uint3 globalInvocationID : SV_DispatchThreadID)
{
    int V_0;
    V_0 = globalInvocationID.x;
    values[V_0] = (cos(values[V_0]) + sin(values[V_0]));
}
";

        AssertFunc((Span<float> values) =>
        {
            var index = (int)GpuIntrinsic.GetGlobalInvocationId().X;
            values[index] = MathF.Cos(values[index]) + MathF.Sin(values[index]);
        }, expectedResult);
    }

    [Test]
    public void CompilesFunction()
    {
        var expectedResult = @"uint un_user_func_Fib(uint n);

RWStructuredBuffer<uint> values : register(u0);
[numthreads(1, 1, 1)]
void main(uint3 globalInvocationID : SV_DispatchThreadID)
{
    int V_0;
    V_0 = globalInvocationID.x;
    values[V_0] = un_user_func_Fib(values[V_0]);
}

uint un_user_func_Fib(uint n)
{
    uint V_0;
    uint V_1;
    bool V_2;
    uint V_3;
    uint V_4;
    uint V_5;
    bool V_6;
    n = (n % 16);
    V_2 = ((n > 1) == false);
    if ((!(!V_2)))
    {
        V_3 = n;
    }
    else
    {
        V_0 = 1;
        V_1 = 1;
        V_4 = 2;
        while (true)
        {
            V_6 = (V_4 < n);
            if ((!V_6))
            {
                break;
            }
            V_5 = V_0;
            V_0 = (V_0 + V_1);
            V_1 = V_5;
            V_4 = (V_4 + 1);
        }
        V_3 = V_0;
    }
    return V_3;
}
";

        AssertFunc((Span<uint> values) =>
        {
            var index = (int)GpuIntrinsic.GetGlobalInvocationId().X;
            values[index] = Fib(values[index]);
        }, expectedResult);
    }

    private static uint Fib(uint n)
    {
        n %= 16;
        if (n <= 1)
        {
            return n;
        }

        var c = 1u;
        var p = 1u;

        for (uint i = 2; i < n; ++i)
        {
            var t = c;
            c += p;
            p = t;
        }

        return c;
    }
}
