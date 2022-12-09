using System.Numerics;
using UraniumCompute.Common.Math;
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
    return ;
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
    return ;
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
    return ;
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
        var expectedResult = @"uint un_user_defined_Fib(uint n);

RWStructuredBuffer<uint> values : register(u0);
[numthreads(1, 1, 1)]
void main(uint3 globalInvocationID : SV_DispatchThreadID)
{
    int V_0;
    V_0 = globalInvocationID.x;
    values[V_0] = un_user_defined_Fib(values[V_0]);
    return ;
}

uint un_user_defined_Fib(uint n)
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

    [Test]
    public void CompilesDoubleReferencedFunction()
    {
        var expectedResult = @"int un_user_defined_Bar();

int un_user_defined_Foo();

[numthreads(1, 1, 1)]
int main(uint3 globalInvocationID : SV_DispatchThreadID)
{
    int V_0;
    int V_1;
    int V_2;
    V_0 = un_user_defined_Foo();
    V_1 = un_user_defined_Bar();
    V_2 = (V_0 + V_1);
    return V_2;
}

int un_user_defined_Bar()
{
    int V_0;
    V_0 = 123;
    return V_0;
}

int un_user_defined_Foo()
{
    int V_0;
    V_0 = (un_user_defined_Bar() * 2);
    return V_0;
}
";

        AssertFunc(() =>
        {
            var a = Foo();
            var b = Bar();
            return a + b;
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

    private static int Foo()
    {
        return Bar() * 2;
    }

    private static int Bar()
    {
        return 123;
    }
    
    [Test]
    public void CompilesVectorDeclaration()
    {
        var expectedResult = @"RWStructuredBuffer<float> values : register(u0);
[numthreads(1, 1, 1)]
void main(uint3 globalInvocationID : SV_DispatchThreadID)
{
    float2 V_0;
    int2 V_1;
    uint2 V_2;
    float3 V_3;
    int3 V_4;
    uint3 V_5;
    float4 V_6;
    int4 V_7;
    uint4 V_8;
    return ;
}
";

        AssertFunc((Span<float> values) =>
        {
            Vector2 q;
            Vector2Int w;
            Vector2Uint e;
            Vector3 r;
            Vector3Int t;
            Vector3Uint y;
            Vector4 u;
            Vector4Int i;
            Vector4Uint o;
        }, expectedResult);
    }
    
    [Test]
    public void CompilesMatrixDeclaration()
    {
        var expectedResult = @"RWStructuredBuffer<float> values : register(u0);
[numthreads(1, 1, 1)]
void main(uint3 globalInvocationID : SV_DispatchThreadID)
{
    float2x2 V_0;
    int2x2 V_1;
    uint2x2 V_2;
    float3x3 V_3;
    int3x3 V_4;
    uint3x3 V_5;
    float4x4 V_6;
    int4x4 V_7;
    uint4x4 V_8;
    return ;
}
";

        AssertFunc((Span<float> values) =>
        {
            Matrix2x2 q;
            Matrix2x2Int w;
            Matrix2x2Uint e;
            Matrix3x3 r;
            Matrix3x3Int t;
            Matrix3x3Uint y;
            Matrix4x4 u;
            Matrix4x4Int i;
            Matrix4x4Uint o;
        }, expectedResult);
    }
}
