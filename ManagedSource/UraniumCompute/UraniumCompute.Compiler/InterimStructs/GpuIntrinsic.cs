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
}
