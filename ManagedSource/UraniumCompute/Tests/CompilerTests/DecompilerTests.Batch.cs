using UraniumCompute.Compiler.InterimStructs;

namespace CompilerTests;

public partial class DecompilerTests
{
    [Test]
    public void CompilesBatches()
    {
        AssertFunc((Span<int> values) =>
            {
                var idx = (int)GpuIntrinsic.GetGlobalInvocationId().X;
                values[idx] = idx + 1000;
            }, """
            void un_user_defined_c_3CCompilesBatchesc_3Ebc_5Fc_5F0c_5F0(uint3 globalInvocationID : SV_DispatchThreadID);
            RWStructuredBuffer<int> values : register(u0);
            [numthreads(1, 1, 1)]
            void main(uint3 globalInvocationID : SV_DispatchThreadID)
            {
                int V_0;
                globalInvocationID.x = (globalInvocationID.x * 16);
                V_0 = 0;
                while ((V_0 < 16))
                {
                    un_user_defined_c_3CCompilesBatchesc_3Ebc_5Fc_5F0c_5F0(globalInvocationID);
                    V_0 = (V_0 + 1);
                    globalInvocationID.x = (globalInvocationID.x + 1);
                }
            }
            void un_user_defined_c_3CCompilesBatchesc_3Ebc_5Fc_5F0c_5F0(uint3 globalInvocationID : SV_DispatchThreadID)
            {
                int V_0;
                V_0 = globalInvocationID.x;
                values[V_0] = (V_0 + 1000);
                return ;
            }
            """, 16);
    }
}
