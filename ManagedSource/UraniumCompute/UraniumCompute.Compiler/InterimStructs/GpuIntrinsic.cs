using System.Runtime.CompilerServices;

namespace UraniumCompute.Compiler.InterimStructs;

public static class GpuIntrinsic
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static Index3D GetGlobalInvocationId()
    {
        // placeholder, used in kernel compilation
        return default;
    }
}
