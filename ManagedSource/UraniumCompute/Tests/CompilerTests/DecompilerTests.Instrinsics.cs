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
            a[index.X] *= 2;
        }, expectedResult);
    }

    [Test]
    public void CompilesIndex3D_GetXYZ()
    {
        var expectedResult = @"[numthreads(1, 1, 1)]
int main(uint3 globalInvocationID : SV_DispatchThreadID)
{
    uint3 V_0;
    int V_1;
    int V_2;
    int V_3;
    int V_4;
    V_0 = globalInvocationID;
    V_1 = V_0.x;
    V_2 = V_0.y;
    V_3 = V_0.z;
    V_4 = (V_1 + V_2);
    return V_4;
}
";
        AssertFunc((Func<int>)(() =>
        {
            var index = GpuIntrinsic.GetGlobalInvocationId();
            var x = index.X;
            var y = index.Y;
            var z = index.Z;
            return x + y;
        }), expectedResult);
    }
}
