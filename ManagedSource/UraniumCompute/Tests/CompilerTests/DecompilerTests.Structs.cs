using UraniumCompute.Compiler.InterimStructs;

namespace CompilerTests;

public struct TestStruct
{
    public float X;
    public float Y;
}

public partial class DecompilerTests
{
    [Test]
    public void CompilesUserStruct()
    {
        var expectedResult = @"struct un_user_defined_TestStruct;

struct un_user_defined_TestStruct
{
    float un_user_defined_X;
    float un_user_defined_Y;
};
[numthreads(1, 1, 1)]
float main(uint3 globalInvocationID : SV_DispatchThreadID)
{
    un_user_defined_TestStruct V_0;
    un_user_defined_TestStruct V_1;
    float V_2;
    V_1.un_user_defined_X = 0.5;
    V_1.un_user_defined_Y = 1.5;
    V_0 = V_1;
    V_2 = (V_0.un_user_defined_X + V_0.un_user_defined_Y);
    return V_2;
}
";

        AssertFunc(() =>
        {
            var s = new TestStruct { X = 0.5f, Y = 1.5f };
            return s.X + s.Y;
        }, expectedResult);
    }

    [Test]
    public void CompilesUserStruct_InParameter()
    {
        var expectedResult = @"struct un_user_defined_TestStruct;

struct un_user_defined_TestStruct
{
    float un_user_defined_X;
    float un_user_defined_Y;
};

RWStructuredBuffer<un_user_defined_TestStruct> values : register(u0);
[numthreads(1, 1, 1)]
void main(uint3 globalInvocationID : SV_DispatchThreadID)
{
    uint V_0;
    V_0 = globalInvocationID.x;
    values[V_0].un_user_defined_X = ((float)((float)V_0));
    values[V_0].un_user_defined_Y = 1.5;
}
";

        AssertFunc((Span<TestStruct> values) =>
        {
            var index = GpuIntrinsic.GetGlobalInvocationId().X;
            values[(int)index].X = index;
            values[(int)index].Y = 1.5f;
        }, expectedResult);
    }
}
