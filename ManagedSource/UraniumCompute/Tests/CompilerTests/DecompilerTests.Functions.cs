using System.Numerics;
using UraniumCompute.Common.Math;
using UraniumCompute.Compiler.InterimStructs;

namespace CompilerTests;

public partial class DecompilerTests
{
    [Test]
    public void CompilesCos()
    {
        AssertFunc((Span<float> values) =>
        {
            var index = (int)GpuIntrinsic.GetGlobalInvocationId().X;
            values[index] = MathF.Cos(values[index]);
        }, """
            RWStructuredBuffer<float> values : register(u0);
            [numthreads(1, 1, 1)]
            void main(uint3 globalInvocationID : SV_DispatchThreadID)
            {
                int V_0;
                V_0 = globalInvocationID.x;
                values[V_0] = cos(values[V_0]);
                return ;
            }
            """);
    }

    [Test]
    public void CompilesMinMax()
    {
        AssertFunc((Span<float> values) =>
        {
            var index = (int)GpuIntrinsic.GetGlobalInvocationId().X;
            values[index] = MathF.Max(values[index], MathF.Min(1, values[index]));
        }, """
            RWStructuredBuffer<float> values : register(u0);
            [numthreads(1, 1, 1)]
            void main(uint3 globalInvocationID : SV_DispatchThreadID)
            {
                int V_0;
                V_0 = globalInvocationID.x;
                values[V_0] = max(values[V_0], min(1, values[V_0]));
                return ;
            }
            """);
    }

    [Test]
    public void CompilesSinCos()
    {
        AssertFunc((Span<float> values) =>
        {
            var index = (int)GpuIntrinsic.GetGlobalInvocationId().X;
            values[index] = MathF.Cos(values[index]) + MathF.Sin(values[index]);
        }, """
            RWStructuredBuffer<float> values : register(u0);
            [numthreads(1, 1, 1)]
            void main(uint3 globalInvocationID : SV_DispatchThreadID)
            {
                int V_0;
                V_0 = globalInvocationID.x;
                values[V_0] = (cos(values[V_0]) + sin(values[V_0]));
                return ;
            }
            """);
    }

    [Test]
    public void CompilesFunction()
    {
        AssertFunc((Span<uint> values) =>
        {
            var index = (int)GpuIntrinsic.GetGlobalInvocationId().X;
            values[index] = Fib(values[index]);
        }, """
            uint un_user_defined_Fib(uint n);
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
            """);
    }

    [Test]
    public void CompilesDoubleReferencedFunction()
    {
        AssertFunc(() =>
        {
            var a = Foo();
            var b = Bar();
            return a + b;
        }, """
            int un_user_defined_Bar();
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
            """);
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
        AssertFunc((Span<float> values) =>
        {
            var q = new Vector2(7, 7);
            Vector2Int w;
            Vector2Uint e;
            Vector3 r;
            Vector3Int t;
            Vector3Uint y;
            Vector4 u;
            Vector4Int i;
            Vector4Uint o;
        }, """
            RWStructuredBuffer<float> values : register(u0);
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
                V_0 = float2(7, 7);
                return ;
            }
            """);
    }

    [Test]
    public void CompilesMatrixDeclaration()
    {
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
        }, """
            RWStructuredBuffer<float> values : register(u0);
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
            """);
    }

    [Test]
    public void CompilesTranspose()
    {
        AssertFunc(() =>
        {
            Matrix4x4Int matrix = default;
            return Matrix4x4Int.Transpose(matrix);
        }, """
            [numthreads(1, 1, 1)]
            int4x4 main(uint3 globalInvocationID : SV_DispatchThreadID)
            {
                int4x4 V_0;
                int4x4 V_1;
                V_1 = transpose(V_0);
                return V_1;
            }
            """);
    }

    [Test]
    public void CompilesDeterminant()
    {
        AssertFunc(() =>
        {
            var matrix = new Matrix2x2Int(1, 2, 3, 4);
            return matrix.GetDeterminant();
        }, """
            [numthreads(1, 1, 1)]
            int main(uint3 globalInvocationID : SV_DispatchThreadID)
            {
                int2x2 V_0;
                int V_1;
                V_0 = int2x2(1, 2, 3, 4);
                V_1 = determinant(V_0);
                return V_1;
            }
            """);
    }

    [Test]
    public void CompilesDot()
    {
        AssertFunc(() =>
        {
            Vector2Int vector = default;
            return Vector2Int.Dot(vector, vector);
        }, """
            [numthreads(1, 1, 1)]
            int main(uint3 globalInvocationID : SV_DispatchThreadID)
            {
                int2 V_0;
                int V_1;
                V_1 = dot(V_0, V_0);
                return V_1;
            }
            """);
    }

    [Test]
    public void CompilesMatrixMathOperators()
    {
        AssertFunc(() =>
        {
            Matrix4x4Int matrix = default;
            var t1 = matrix + matrix;
            t1 = matrix - matrix;
            t1 = matrix * matrix;
            var t2 = matrix * 9;
            return t1;
        }, """
            [numthreads(1, 1, 1)]
            int4x4 main(uint3 globalInvocationID : SV_DispatchThreadID)
            {
                int4x4 V_0;
                int4x4 V_1;
                int4x4 V_2;
                int4x4 V_3;
                V_1 = (V_0 + V_0);
                V_1 = (V_0 - V_0);
                V_1 = (V_0 * V_0);
                V_2 = (V_0 * 9);
                V_3 = V_1;
                return V_3;
            }
            """);
    }

    [Test]
    public void CompilesVectorMathOperators()
    {
        AssertFunc(() =>
        {
            Vector2Int vector = default;
            var t1 = vector + vector;
            t1 = vector - vector;
            t1 = vector * vector;
            t1 = vector / vector;
            var t2 = vector * 6;
            return t1;
        }, """
            [numthreads(1, 1, 1)]
            int2 main(uint3 globalInvocationID : SV_DispatchThreadID)
            {
                int2 V_0;
                int2 V_1;
                int2 V_2;
                int2 V_3;
                V_1 = (V_0 + V_0);
                V_1 = (V_0 - V_0);
                V_1 = (V_0 * V_0);
                V_1 = (V_0 / V_0);
                V_2 = (V_0 * 6);
                V_3 = V_1;
                return V_3;
            }
            """);
    }

    [Test]
    public void CompilesRefParameters()
    {
        AssertFunc(() =>
        {
            var a = new Vector3();
            UseRefVector(ref a);
            UseOutVector1(out var b);
            UseOutVector2(out var c);
            var d = b + c;
            return UseInVector(in d);
        }, """
            float un_user_defined_UseInVector(inout float3 v);
            void un_user_defined_UseOutVector2(inout float3 v);
            void un_user_defined_UseOutVector1(inout float3 v);
            void un_user_defined_UseRefVector(inout float3 v);
            [numthreads(1, 1, 1)]
            float main(uint3 globalInvocationID : SV_DispatchThreadID) {
                float3 V_0;
                float3 V_1;
                float3 V_2;
                float3 V_3;
                float V_4;
                un_user_defined_UseRefVector(V_0);
                un_user_defined_UseOutVector1(V_1);
                un_user_defined_UseOutVector2(V_2);
                V_3 = (V_1 + V_2);
                V_4 = un_user_defined_UseInVector(V_3);
                return V_4;
            }
            float un_user_defined_UseInVector(inout float3 v) {
                float V_0;
                V_0 = ((v.x + v.y) + v.z);
                return V_0;
            }
            void un_user_defined_UseOutVector2(inout float3 v) {
                v.x = 1;
                v.y = 2;
                v.z = 3;
                return ;
            }
            void un_user_defined_UseOutVector1(inout float3 v) {
                float3 V_0;
                V_0.x = 1;
                V_0.y = 2;
                V_0.z = 3;
                v = V_0;
                return ;
            }
            void un_user_defined_UseRefVector(inout float3 v) {
                v = (0.2 * v);
                v.x = 1;
                return ;
            }
            """);
    }

    private static void UseRefVector(ref Vector3 v)
    {
        v = 0.2f * v;
        v.X = 1;
    }

    private static void UseOutVector1(out Vector3 v)
    {
        v = new Vector3
        {
            X = 1,
            Y = 2,
            Z = 3
        };
    }

    private static void UseOutVector2(out Vector3 v)
    {
        v.X = 1;
        v.Y = 2;
        v.Z = 3;
    }

    private static float UseInVector(in Vector3 v)
    {
        return v.X + v.Y + v.Z;
    }
}
