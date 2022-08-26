using System.Runtime.InteropServices;
using UraniumCompute.Acceleration;
using UraniumCompute.Memory;

namespace UraniumCompute.Backend;

public sealed class ComputeDevice : UnObject
{
    internal ComputeDevice(IntPtr handle) : base(handle)
    {
    }

    public ResultCode Init(in Desc desc)
    {
        return IComputeDevice_Init(Handle, in desc);
    }

    [StructLayout(LayoutKind.Sequential)]
    public readonly record struct Desc(int AdapterId);
    
    [DllImport("UnCompute")]
    private static extern ResultCode IComputeDevice_Init(IntPtr self, in Desc desc);
    
    [DllImport("UnCompute")]
    private static extern ResultCode IComputeDevice_CreateBuffer(IntPtr self, out IntPtr buffer);
    
    [DllImport("UnCompute")]
    private static extern ResultCode IComputeDevice_CreateMemory(IntPtr self, out IntPtr buffer);
}
