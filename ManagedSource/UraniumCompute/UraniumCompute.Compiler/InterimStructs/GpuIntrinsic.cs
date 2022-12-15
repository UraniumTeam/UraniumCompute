using System.Runtime.CompilerServices;
using UraniumCompute.Common.Math;

namespace UraniumCompute.Compiler.InterimStructs;

/// placeholders, used in kernel compilation
public static class GpuIntrinsic
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static Vector3Uint GetGlobalInvocationId() => default;
}
