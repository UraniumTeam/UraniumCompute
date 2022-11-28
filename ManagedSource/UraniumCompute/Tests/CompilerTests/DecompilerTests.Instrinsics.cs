using UraniumCompute.Common.Math;
using UraniumCompute.Compiler.InterimStructs;

namespace CompilerTests;

public partial class DecompilerTests
{
    [Test]
    public void CompilesGetGlobalInvocationId()
    {
        var expectedResult = @"RWStructuredBuffer<int> a : register(u0);
[numthreads(1, 1, 1)]
void main(uint3 globalInvocationID : SV_DispatchThreadID)
{
    uint3 V_0;
    V_0 = globalInvocationID;
    a[V_0.x] = (a[V_0.x] * 2);
}
";
        AssertFunc((Span<int> a) =>
        {
            var index = GpuIntrinsic.GetGlobalInvocationId();
            a[(int)index.X] *= 2;
        }, expectedResult);
    }

    [Test]
    public void CompilesVector3Uint_New()
    {
        var expectedResult = @"[numthreads(1, 1, 1)]
void main(uint3 globalInvocationID : SV_DispatchThreadID)
{
    uint3 V_0;
    uint V_1;
    uint3 V_2;
    V_2.x = 1;
    V_2.y = 2;
    V_2.z = 3;
    V_0 = V_2;
    V_1 = (V_0.x + V_0.y);
    return V_2;
}
";
        AssertFunc(() =>
        {
            var v = new Vector3Uint { X = 1, Y = 2, Z = 3 };
            var s = v.X + v.Y;
        }, expectedResult);
    }

    [Test]
    public void CompilesVector3Uint_GetXYZ()
    {
        var expectedResult = @"[numthreads(1, 1, 1)]
uint main(uint3 globalInvocationID : SV_DispatchThreadID)
{
    uint3 V_0;
    uint V_1;
    uint V_2;
    uint V_3;
    uint V_4;
    V_0 = globalInvocationID;
    V_1 = V_0.x;
    V_2 = V_0.y;
    V_3 = V_0.z;
    V_4 = (V_1 + V_2);
    return V_4;
}
";
        AssertFunc((Func<uint>)(() =>
        {
            var index = GpuIntrinsic.GetGlobalInvocationId();
            var x = index.X;
            var y = index.Y;
            var z = index.Z;
            return x + y;
        }), expectedResult);
    }
}
