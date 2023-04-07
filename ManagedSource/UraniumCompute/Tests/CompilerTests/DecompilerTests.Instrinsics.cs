using UraniumCompute.Common.Math;
using UraniumCompute.Compiler.InterimStructs;

namespace CompilerTests;

public partial class DecompilerTests
{
    [Test]
    public void CompilesGetGlobalInvocationId()
    {
        AssertFunc((Span<int> a) =>
        {
            var index = GpuIntrinsic.GetGlobalInvocationId();
            a[(int)index.X] *= 2;
        }, """
            RWStructuredBuffer<int> a : register(u0);
            [numthreads(1, 1, 1)]
            void main(uint3 globalInvocationID : SV_DispatchThreadID)
            {
                uint3 V_0;
                V_0 = globalInvocationID;
                a[V_0.x] = (a[V_0.x] * 2);
                return ;
            }
            """);
    }

    [Test]
    public void CompilesVector3Uint_New()
    {
        AssertFunc(() =>
        {
            var v = new Vector3Uint { X = 1, Y = 2, Z = 3 };
            var s = v.X + v.Y;
        }, """
            [numthreads(1, 1, 1)]
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
                return ;
            }
            """);
    }

    [Test]
    public void CompilesVector3Uint_GetXYZ()
    {
        AssertFunc((Func<uint>)(() =>
        {
            var index = GpuIntrinsic.GetGlobalInvocationId();
            var x = index.X;
            var y = index.Y;
            var z = index.Z;
            return x + y;
        }), """
            [numthreads(1, 1, 1)]
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
            """);
    }
}
