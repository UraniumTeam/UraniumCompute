using UraniumCompute.Compiler.InterimStructs;

namespace CompilerTests;

public partial class DecompilerTests
{
    [Test]
    public void CompilesIfStatement()
    {
        var expectedResult = @"[numthreads(1, 1, 1)]
int main(uint3 globalInvocationID : SV_DispatchThreadID)
{
    int V_0;
    bool V_1;
    uint3 V_2;
    int V_3;
    V_0 = 0;
    V_2 = globalInvocationID;
    V_1 = (V_2.x > 10);
    if (!(V_1 == false))
    {
        V_0 = 1;
    }
    V_3 = V_0;
    return V_3;
}
";
        AssertFunc(() =>
        {
            var a = 0;
            if (GpuIntrinsic.GetGlobalInvocationId().X > 10)
            {
                a = 1;
            }

            return a;
        }, expectedResult);
    }

    [Test]
    public void CompilesNestedIfStatement()
    {
        var expectedResult = @"[numthreads(1, 1, 1)]
int main(uint3 globalInvocationID : SV_DispatchThreadID)
{
    int V_0;
    bool V_1;
    uint3 V_2;
    bool V_3;
    int V_4;
    V_0 = 0;
    V_2 = globalInvocationID;
    V_1 = (V_2.x > 10);
    if (!(V_1 == false))
    {
        V_0 = 1;
        V_2 = globalInvocationID;
        V_3 = (V_2.x > 100);
        if (!(V_3 == false))
        {
            V_0 = 2;
        }
    }
    V_4 = V_0;
    return V_4;
}
";

        AssertFunc(() =>
        {
            var a = 0;
            if (GpuIntrinsic.GetGlobalInvocationId().X > 10)
            {
                a = 1;
                if (GpuIntrinsic.GetGlobalInvocationId().X > 100)
                {
                    a = 2;
                }
            }

            return a;
        }, expectedResult);
    }

    [Test]
    public void CompilesIfElseStatement()
    {
        var expectedResult = @"[numthreads(1, 1, 1)]
int main(uint3 globalInvocationID : SV_DispatchThreadID)
{
    int V_0;
    bool V_1;
    uint3 V_2;
    int V_3;
    V_0 = 0;
    V_2 = globalInvocationID;
    V_1 = (V_2.x > 10);
    if (!(V_1 == false))
    {
        V_0 = 1;
    }
    else
    {
        V_0 = 2;
    }
    V_3 = V_0;
    return V_3;
}
";

        AssertFunc(() =>
        {
            var a = 0;
            if (GpuIntrinsic.GetGlobalInvocationId().X > 10)
            {
                a = 1;
            }
            else
            {
                a = 2;
            }

            return a;
        }, expectedResult);
    }
}
