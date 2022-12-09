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
    
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static Matrix2x2 Transpose(Matrix2x2 value)
    {
        return Matrix2x2.Transpose(value);
    }
        
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static Matrix2x2Int Transpose(Matrix2x2Int value)
    {
        return Matrix2x2Int.Transpose(value);
    }
        
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static Matrix2x2Uint Transpose(Matrix2x2Uint value)
    {
        return Matrix2x2Uint.Transpose(value);
    }
        
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static Matrix3x3 Transpose(Matrix3x3 value)
    {
        return Matrix3x3.Transpose(value);
    }
        
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static Matrix3x3Int Transpose(Matrix3x3Int value)
    {        return Matrix3x3Int.Transpose(value);
    }
        
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static Matrix3x3Uint Transpose(Matrix3x3Uint value)
    {
        return Matrix3x3Uint.Transpose(value);
    }
    
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static Matrix4x4 Transpose(Matrix4x4 value)
    {
        return Matrix4x4.Transpose(value);
    }
        
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static Matrix4x4Int Transpose(Matrix4x4Int value)
    {
        return Matrix4x4Int.Transpose(value);
    }
        
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static Matrix4x4Uint Transpose(Matrix4x4Uint value)
    {
        return Matrix4x4Uint.Transpose(value);
    }
}
