using System.Numerics;
using System.Runtime.CompilerServices;
using UraniumCompute.Common.Math;

namespace UraniumCompute.Compiler.InterimStructs;

public static class GpuIntrinsic
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static Vector3Uint GetGlobalInvocationId()
    {
        // placeholder, used in kernel compilation
        return default;
    }
    
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static float Determinant(Matrix2x2 value)
    {
        return value.GetDeterminant();
    }
        
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int Determinant(Matrix2x2Int value)
    {
        return value.GetDeterminant();
    }
        
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static uint Determinant(Matrix2x2Uint value)
    {
        return value.GetDeterminant();
    }
        
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static float Determinant(Matrix3x3 value)
    {
        return value.GetDeterminant();
    }
        
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int Determinant(Matrix3x3Int value)
    {
        return value.GetDeterminant();
    }
        
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static uint Determinant(Matrix3x3Uint value)
    {
        return value.GetDeterminant();
    }
    
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static float Determinant(Matrix4x4 value)
    {
        return value.GetDeterminant();
    }
        
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int Determinant(Matrix4x4Int value)
    {
        return value.GetDeterminant();
    }
        
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static uint Determinant(Matrix4x4Uint value)
    {
        return value.GetDeterminant();
    }
}
