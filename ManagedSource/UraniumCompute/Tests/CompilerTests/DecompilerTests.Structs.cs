﻿using UraniumCompute.Common.Math;
using UraniumCompute.Compiler.InterimStructs;

namespace CompilerTests;

public enum TestEnum
{
    T0,
    T1,
    T2,
    T123 = 123
}

public struct TestStruct
{
    public float X;
    public float Y;
}

public struct TestStructOfStructs
{
    public TestStruct S1;
    public TestStruct S2;
    public TestEnum E;
}

public struct ConstantBuffer
{
    public TestStruct Ts;
}

public partial class DecompilerTests
{
    [Test]
    public void CompilesUserEnum()
    {
        AssertFunc((Span<TestEnum> values) =>
        {
            values[1] = values[0];
            if (values[0] == TestEnum.T123)
            {
                return -1;
            }

            return (int)values[0];
        }, """
            RWStructuredBuffer<int> values : register(u0);
            [numthreads(1, 1, 1)]
            int main(uint3 globalInvocationID : SV_DispatchThreadID)
            {
                bool V_0;
                int V_1;
                values[1] = values[0];
                V_0 = (values[0] == 123);
                if ((!(!V_0)))
                {
                    V_1 = -1;
                }
                else
                {
                    V_1 = values[0];
                }
                return V_1;
            }
            """);
    }

    [Test]
    public void CompilesUserStruct()
    {
        AssertFunc(() =>
        {
            var s = new TestStruct { X = 0.5f, Y = 1.5f };
            var a = s.X + s.Y;
        }, """
            struct un_user_defined_TestStruct;
            
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
            """);
    }

    [Test]
    public void CompilesUserStruct_InParameter()
    {
        AssertFunc((Span<TestStruct> values) =>
        {
            var index = GpuIntrinsic.GetGlobalInvocationId().X;
            values[(int)index].X = index;
            values[(int)index].Y = 1.5f;
        }, """
            struct un_user_defined_TestStruct;
            
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
            """);
    }

    [Test]
    public void CompilesUserStruct_DoubleReference()
    {
        AssertFunc((Span<TestStructOfStructs> values) =>
        {
            var s = new TestStruct { X = 1, Y = 2 };
            var index = GpuIntrinsic.GetGlobalInvocationId().X;
            values[(int)index].S1.X = index;
            values[(int)index].S1.Y = 1.5f;
            values[(int)index].S2 = s;
            values[(int)index].E = TestEnum.T123;
        }, """
            struct un_user_defined_TestStruct;
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
                int un_user_defined_E;
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
                values[V_1].un_user_defined_E = 123;
                return ;
            }
            """);
    }

    [Test]
    public void CompilesCbufferStruct()
    {
        AssertFunc((Span<int> buffer, ConstantBuffer constants) =>
        {
            var x = constants.Ts;
            return x.X + buffer[0];
        }, """
            struct un_user_defined_TestStruct;
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
                un_user_defined_ConstantBuffer constants;
            };
            [numthreads(1, 1, 1)]
            float main(uint3 globalInvocationID : SV_DispatchThreadID)
            {
                un_user_defined_TestStruct V_0;
                float V_1;
                V_0 = constants.un_user_defined_Ts;
                V_1 = (V_0.un_user_defined_X + ((float)buffer[0]));
                return V_1;
            }
            """);
    }

    [Test]
    public void CompilesGpuRandomStruct()
    {
        AssertFunc((Span<float> buffer) =>
        {
            var random = new GpuRandom { Seed = 123 };
            buffer[0] = (int)random.NextFloat() + random.NextInt();
            var v = random.InsideUnitSphere();
        }, """
            struct un_user_defined_GpuRandom;
            float3 un_user_defined_InsideUnitSphere(un_user_defined_GpuRandom un_Self);
            float un_user_defined_NextFloat(un_user_defined_GpuRandom un_Self);
            void un_user_defined_Cycle(un_user_defined_GpuRandom un_Self);
            int un_user_defined_NextInt(un_user_defined_GpuRandom un_Self);
            struct un_user_defined_GpuRandom
            {
                int un_user_defined_Seed;
            };
            RWStructuredBuffer<float> buffer : register(u0);
            [numthreads(1, 1, 1)]
            void main(uint3 globalInvocationID : SV_DispatchThreadID)
            {
                un_user_defined_GpuRandom V_0;
                float3 V_1;
                un_user_defined_GpuRandom V_2;
                V_2.un_user_defined_Seed = 123;
                V_0 = V_2;
                buffer[0] = ((float)(((int)un_user_defined_NextFloat(V_0)) + un_user_defined_NextInt(V_0)));
                V_1 = un_user_defined_InsideUnitSphere(V_0);
                return ;
            }
            float3 un_user_defined_InsideUnitSphere(un_user_defined_GpuRandom un_Self)
            {
                float V_0;
                float V_1;
                float V_2;
                float V_3;
                float V_4;
                float V_5;
                float3 V_6;
                V_0 = ((un_user_defined_NextFloat(un_Self) * 3.1415927) * 2);
                V_1 = (un_user_defined_NextFloat(un_Self) * 3.1415927);
                V_2 = un_user_defined_NextFloat(un_Self);
                V_3 = ((V_2 * sin(V_1)) * cos(V_0));
                V_4 = ((V_2 * sin(V_1)) * sin(V_0));
                V_5 = (V_2 * cos(V_1));
                V_6 = float3(V_3, V_4, V_5);
                return V_6;
            }
            float un_user_defined_NextFloat(un_user_defined_GpuRandom un_Self)
            {
                float V_0;
                un_user_defined_Cycle(un_Self);
                V_0 = (4.656613e-10 * ((float)un_Self.un_user_defined_Seed));
                return V_0;
            }
            void un_user_defined_Cycle(un_user_defined_GpuRandom un_Self)
            {
                int V_0;
                bool V_1;
                un_Self.un_user_defined_Seed = (un_Self.un_user_defined_Seed ^ 123459876);
                V_0 = ((int)(((float)un_Self.un_user_defined_Seed) / 127773));
                un_Self.un_user_defined_Seed = ((int)((16807 * (((float)un_Self.un_user_defined_Seed) - (((float)V_0) * 127773))) - ((float)(2836 * V_0))));
                V_1 = (un_Self.un_user_defined_Seed < 0);
                if ((!(!V_1)))
                {
                    un_Self.un_user_defined_Seed = (un_Self.un_user_defined_Seed + 2147483647);
                }
                un_Self.un_user_defined_Seed = (un_Self.un_user_defined_Seed ^ 123459876);
                return ;
            }
            int un_user_defined_NextInt(un_user_defined_GpuRandom un_Self)
            {
                int V_0;
                un_user_defined_Cycle(un_Self);
                V_0 = un_Self.un_user_defined_Seed;
                return V_0;
            }
            """);
    }
}
