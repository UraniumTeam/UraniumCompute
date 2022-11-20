using UraniumCompute.Compiler.InterimStructs;

namespace CompilerTests;

public partial class DecompilerTests
{
    [Test]
    public void CompilesIfStatement()
    {
        var expectedResult = "[numthreads(1, 1, 1)] " +
                             "int main(uint3 globalInvocationID : SV_DispatchThreadID) " +
                             "{ " +
                             "if (globalInvocationID.x > 10) { " +
                             "return 0; " +
                             "} " +
                             "return 1; }";
        AssertFunc(() =>
        {
            if (GpuIntrinsic.GetGlobalInvocationId().X > 10)
            {
                return 0;
            }

            return 1;
        }, expectedResult);
    }
}
