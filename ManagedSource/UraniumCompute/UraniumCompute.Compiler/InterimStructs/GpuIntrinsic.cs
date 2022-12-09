using System.Numerics;
using System.Runtime.CompilerServices;
using UraniumCompute.Common.Math;

namespace UraniumCompute.Compiler.InterimStructs;

/// placeholders, used in kernel compilation
public static class GpuIntrinsic
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static Vector3Uint GetGlobalInvocationId() => default;

    #region determinant

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static float Determinant(Matrix2x2 value) => default;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int Determinant(Matrix2x2Int value) => default;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static uint Determinant(Matrix2x2Uint value) => default;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static float Determinant(Matrix3x3 value) => default;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int Determinant(Matrix3x3Int value) => default;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static uint Determinant(Matrix3x3Uint value) => default;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static float Determinant(Matrix4x4 value) => default;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int Determinant(Matrix4x4Int value) => default;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static uint Determinant(Matrix4x4Uint value) => default;

    #endregion

    #region transpose

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static Matrix2x2 Transpose(Matrix2x2 value) => default;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static Matrix2x2Int Transpose(Matrix2x2Int value) => default;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static Matrix2x2Uint Transpose(Matrix2x2Uint value) => default;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static Matrix3x3 Transpose(Matrix3x3 value) => default;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static Matrix3x3Int Transpose(Matrix3x3Int value) => default;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static Matrix3x3Uint Transpose(Matrix3x3Uint value) => default;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static Matrix4x4 Transpose(Matrix4x4 value) => default;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static Matrix4x4Int Transpose(Matrix4x4Int value) => default;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static Matrix4x4Uint Transpose(Matrix4x4Uint value) => default;

    #endregion

    #region abs

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static Vector2 Abs(Vector2 value) => default;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static Vector3 Abs(Vector3 value) => default;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static Vector4 Abs(Vector4 value) => default;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static Vector2Int Abs(Vector2Int value) => default;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static Vector3Int Abs(Vector3Int value) => default;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static Vector4Int Abs(Vector4Int value) => default;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static Matrix2x2 Abs(Matrix2x2 value) => default;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static Matrix3x3 Abs(Matrix3x3 value) => default;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static Matrix4x4 Abs(Matrix4x4 value) => default;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static Matrix2x2Int Abs(Matrix2x2Int value) => default;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static Matrix3x3Int Abs(Matrix3x3Int value) => default;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static Matrix4x4Int Abs(Matrix4x4Int value) => default;

    #endregion
}
