﻿using UraniumCompute.Compiler.InterimStructs;

namespace CompilerTests;

public partial class DecompilerTests
{
    [Test]
    public void CompilesIfStatement()
    {
        AssertFunc(() =>
        {
            var a = 0;
            if (GpuIntrinsic.GetGlobalInvocationId().X > 10)
            {
                a = 1;
            }

            return a;
        }, """
            [numthreads(1, 1, 1)]
            int main(uint3 globalInvocationID : SV_DispatchThreadID)
            {
                int V_0;
                bool V_1;
                int V_2;
                V_0 = 0;
                V_1 = (globalInvocationID.x > 10);
                if ((!(!V_1)))
                {
                    V_0 = 1;
                }
                V_2 = V_0;
                return V_2;
            }
            """);
    }

    [Test]
    public void CompilesNestedIfStatement()
    {
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
        }, """
            [numthreads(1, 1, 1)]
            int main(uint3 globalInvocationID : SV_DispatchThreadID)
            {
                int V_0;
                bool V_1;
                bool V_2;
                int V_3;
                V_0 = 0;
                V_1 = (globalInvocationID.x > 10);
                if ((!(!V_1)))
                {
                    V_0 = 1;
                    V_2 = (globalInvocationID.x > 100);
                    if ((!(!V_2)))
                    {
                        V_0 = 2;
                    }
                }
                V_3 = V_0;
                return V_3;
            }
            """);
    }

    [Test]
    public void CompilesIfElseStatement()
    {
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
        }, """
            [numthreads(1, 1, 1)]
            int main(uint3 globalInvocationID : SV_DispatchThreadID)
            {
                int V_0;
                bool V_1;
                int V_2;
                V_0 = 0;
                V_1 = (globalInvocationID.x > 10);
                if ((!(!V_1)))
                {
                    V_0 = 1;
                }
                else
                {
                    V_0 = 2;
                }
                V_2 = V_0;
                return V_2;
            }
            """);
    }

    [Test]
    public void CompilesNestedIfElseStatement()
    {
        AssertFunc((Span<int> a) =>
        {
            var result = 0;
            if (a[0] < 10)
            {
                result += 1;
                if (a[0] > 1)
                {
                    result += 2;
                }
                else
                {
                    result += 1;
                }

                result -= 200;
            }
            else
            {
                result += 4;
                if (a[0] > 100)
                {
                    result += 1;
                }
                else
                {
                    result += 10;
                }

                if (a[result] > 1)
                {
                    result -= 1200;
                }
            }

            return result;
        }, """
            RWStructuredBuffer<int> a : register(u0);
            [numthreads(1, 1, 1)]
            int main(uint3 globalInvocationID : SV_DispatchThreadID)
            {
                int V_0;
                bool V_1;
                bool V_2;
                bool V_3;
                bool V_4;
                int V_5;
                V_0 = 0;
                V_1 = (a[0] < 10);
                if ((!(!V_1)))
                {
                    V_0 = (V_0 + 1);
                    V_2 = (a[0] > 1);
                    if ((!(!V_2)))
                    {
                        V_0 = (V_0 + 2);
                    }
                    else
                    {
                        V_0 = (V_0 + 1);
                    }
                    V_0 = (V_0 - 200);
                }
                else
                {
                    V_0 = (V_0 + 4);
                    V_3 = (a[0] > 100);
                    if ((!(!V_3)))
                    {
                        V_0 = (V_0 + 1);
                    }
                    else
                    {
                        V_0 = (V_0 + 10);
                    }
                    V_4 = (a[V_0] > 1);
                    if ((!(!V_4)))
                    {
                        V_0 = (V_0 - 1200);
                    }
                }
                V_5 = V_0;
                return V_5;
            }
            """);
    }

    [Test]
    public void CompilesWhileLoop()
    {
        AssertFunc((Span<int> a) =>
        {
            var result = 0;
            while (a[0] > 10)
            {
                a[0] /= 2;
                result++;
            }

            return result;
        }, """
            RWStructuredBuffer<int> a : register(u0);
            [numthreads(1, 1, 1)]
            int main(uint3 globalInvocationID : SV_DispatchThreadID)
            {
                int V_0;
                bool V_1;
                int V_2;
                V_0 = 0;
                while (true)
                {
                    V_1 = (a[0] > 10);
                    if ((!V_1))
                    {
                        break;
                    }
                    a[0] = (a[0] / 2);
                    V_0 = (V_0 + 1);
                }
                V_2 = V_0;
                return V_2;
            }
            """);
    }

    [Test]
    public void CompilesForLoop()
    {
        AssertFunc((Span<int> a) =>
        {
            for (var i = 0; i < 10; ++i)
            {
                a[i] = i;
            }
        }, """
            RWStructuredBuffer<int> a : register(u0);
            [numthreads(1, 1, 1)]
            void main(uint3 globalInvocationID : SV_DispatchThreadID)
            {
                int V_0;
                bool V_1;
                V_0 = 0;
                while (true)
                {
                    V_1 = (V_0 < 10);
                    if ((!V_1))
                    {
                        break;
                    }
                    a[V_0] = V_0;
                    V_0 = (V_0 + 1);
                }
                return ;
            }
            """);
    }

    [Test]
    public void CompilesNestedLoops()
    {
        AssertFunc((Span<int> a) =>
        {
            for (var i = 0; i < 10; i++)
            for (var j = 0; j < 10; j++)
            {
                a[i + j] += i + j;
            }
        }, """
            RWStructuredBuffer<int> a : register(u0);
            [numthreads(1, 1, 1)]
            void main(uint3 globalInvocationID : SV_DispatchThreadID)
            {
                int V_0;
                int V_1;
                bool V_2;
                bool V_3;
                V_0 = 0;
                while (true)
                {
                    V_3 = (V_0 < 10);
                    if ((!V_3))
                    {
                        break;
                    }
                    V_1 = 0;
                    while (true)
                    {
                        V_2 = (V_1 < 10);
                        if ((!V_2))
                        {
                            break;
                        }
                        a[(V_0 + V_1)] = (a[(V_0 + V_1)] + (V_0 + V_1));
                        V_1 = (V_1 + 1);
                    }
                    V_0 = (V_0 + 1);
                }
                return ;
            }
            """);
    }

    [Test]
    public void CompilesNestedLoopAndIf()
    {
        AssertFunc((Span<int> a) =>
        {
            for (var i = 0; i < 10; i++)
            for (var j = 0; j < 10; j++)
            {
                if (i != j)
                {
                    a[i + j] += i + j;
                }
                else
                {
                    a[i + j] = -1;
                }
            }
        }, """
            RWStructuredBuffer<int> a : register(u0);
            [numthreads(1, 1, 1)]
            void main(uint3 globalInvocationID : SV_DispatchThreadID)
            {
                int V_0;
                int V_1;
                bool V_2;
                bool V_3;
                bool V_4;
                V_0 = 0;
                while (true)
                {
                    V_4 = (V_0 < 10);
                    if ((!V_4))
                    {
                        break;
                    }
                    V_1 = 0;
                    while (true)
                    {
                        V_3 = (V_1 < 10);
                        if ((!V_3))
                        {
                            break;
                        }
                        V_2 = ((V_0 == V_1) == false);
                        if ((!(!V_2)))
                        {
                            a[(V_0 + V_1)] = (a[(V_0 + V_1)] + (V_0 + V_1));
                        }
                        else
                        {
                            a[(V_0 + V_1)] = -1;
                        }
                        V_1 = (V_1 + 1);
                    }
                    V_0 = (V_0 + 1);
                }
                return ;
            }
            """);
    }

    [Test]
    public void CompilesNestedLoopIfElse()
    {
        AssertFunc(() =>
        {
            var a = 0;
            for (var i = 0; i < 10; ++i)
            {
                while (GpuIntrinsic.GetGlobalInvocationId().X > 5)
                {
                    if (i > 3)
                    {
                        a++;
                    }
                    else
                    {
                        a += 2;
                    }

                    a /= 10;
                }

                a *= 20;
            }

            return a;
        }, """
            [numthreads(1, 1, 1)]
            int main(uint3 globalInvocationID : SV_DispatchThreadID)
            {
                int V_0;
                int V_1;
                bool V_2;
                bool V_3;
                bool V_4;
                int V_5;
                V_0 = 0;
                V_1 = 0;
                while (true)
                {
                    V_4 = (V_1 < 10);
                    if ((!V_4))
                    {
                        break;
                    }
                    while (true)
                    {
                        V_3 = (globalInvocationID.x > 5);
                        if ((!V_3))
                        {
                            break;
                        }
                        V_2 = (V_1 > 3);
                        if ((!(!V_2)))
                        {
                            V_0 = (V_0 + 1);
                        }
                        else
                        {
                            V_0 = (V_0 + 2);
                        }
                        V_0 = (V_0 / 10);
                    }
                    V_0 = (V_0 * 20);
                    V_1 = (V_1 + 1);
                }
                V_5 = V_0;
                return V_5;
            }
            """);
    }
}
