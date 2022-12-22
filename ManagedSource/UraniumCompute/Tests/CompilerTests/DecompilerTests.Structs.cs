using UraniumCompute.Compiler.InterimStructs;

namespace CompilerTests;

public struct TestStruct
{
    public float X;
    public float Y;
}

public struct TestStructOfStructs
{
    public TestStruct S1;
    public TestStruct S2;
}

public struct ConstantBuffer
{
    public TestStruct Ts;
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
void main(uint3 globalInvocationID : SV_DispatchThreadID)
{
    un_user_defined_TestStruct V_0;
    float V_1;
    un_user_defined_TestStruct V_2;
    V_2.un_user_defined_X = 0.5;
    V_2.un_user_defined_Y = 1.5;
    V_0 = V_2;
    V_1 = (V_0.un_user_defined_X + V_0.un_user_defined_Y);
    return ;
}
";

        AssertFunc(() =>
        {
            var s = new TestStruct { X = 0.5f, Y = 1.5f };
            var a = s.X + s.Y;
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
    return ;
}
";

        AssertFunc((Span<TestStruct> values) =>
        {
            var index = GpuIntrinsic.GetGlobalInvocationId().X;
            values[(int)index].X = index;
            values[(int)index].Y = 1.5f;
        }, expectedResult);
    }

    [Test]
    public void CompilesUserStruct_DoubleReference()
    {
        var expectedResult = @"struct un_user_defined_TestStruct;
struct un_user_defined_TestStructOfStructs;

struct un_user_defined_TestStruct
{
    float un_user_defined_X;
    float un_user_defined_Y;
};

struct un_user_defined_TestStructOfStructs
{
    un_user_defined_TestStruct un_user_defined_S1;
    un_user_defined_TestStruct un_user_defined_S2;
};

RWStructuredBuffer<un_user_defined_TestStructOfStructs> values : register(u0);
[numthreads(1, 1, 1)]
void main(uint3 globalInvocationID : SV_DispatchThreadID)
{
    un_user_defined_TestStruct V_0;
    uint V_1;
    un_user_defined_TestStruct V_2;
    V_2.un_user_defined_X = 1;
    V_2.un_user_defined_Y = 2;
    V_0 = V_2;
    V_1 = globalInvocationID.x;
    values[V_1].un_user_defined_S1.un_user_defined_X = ((float)((float)V_1));
    values[V_1].un_user_defined_S1.un_user_defined_Y = 1.5;
    values[V_1].un_user_defined_S2 = V_0;
    return ;
}
";

        AssertFunc((Span<TestStructOfStructs> values) =>
        {
            var s = new TestStruct { X = 1, Y = 2 };
            var index = GpuIntrinsic.GetGlobalInvocationId().X;
            values[(int)index].S1.X = index;
            values[(int)index].S1.Y = 1.5f;
            values[(int)index].S2 = s;
        }, expectedResult);
    }
    
    [Test]
    public void CompilesCbufferStruct()
    {
        var expectedResult = @"struct un_user_defined_TestStruct;
struct un_user_defined_ConstantBuffer;

struct un_user_defined_TestStruct
{
    float un_user_defined_X;
    float un_user_defined_Y;
};

struct un_user_defined_ConstantBuffer
{
    un_user_defined_TestStruct un_user_defined_Ts;
};

RWStructuredBuffer<int> buffer : register(u0);

cbuffer Constants : register(b1)
{
    ConstantBuffer constants;
};

[numthreads(1, 1, 1)]
float main(uint3 globalInvocationID : SV_DispatchThreadID)
{
    un_user_defined_TestStruct V_0;
    float V_1;
    V_0 = constants.ts;
    V_1 = (V_0.un_user_defined_X + ((float)buffer[0]));
    return V_1;
}

";

        AssertFunc((Span<int> buffer, ConstantBuffer constants) =>
        {
            var x = constants.Ts;
            return x.X + buffer[0];
        }, expectedResult);
    }
}
