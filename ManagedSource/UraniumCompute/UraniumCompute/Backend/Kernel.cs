using System.Runtime.InteropServices;
using UraniumCompute.Acceleration;
using UraniumCompute.Containers;
using UraniumCompute.Memory;

namespace UraniumCompute.Backend;

public sealed class Kernel : DeviceObject<Kernel.Desc>
{
    public override Desc Descriptor
    {
        get
        {
            IKernel_GetDesc(Handle, out var desc);
            return new Desc(desc.Name, resourceBinding!, new NativeArray<byte>());
        }
    }

    private ResourceBinding? resourceBinding;

    internal Kernel(IntPtr handle) : base(handle)
    {
    }

    public override unsafe void Init(in Desc desc)
    {
        resourceBinding = desc.ResourceBinding;
        IKernel_Init(Handle, new DescNative(desc.Name, desc.ResourceBinding.Handle, new ArraySliceBase
        {
            pBegin = (sbyte*)desc.Bytecode.NativePointer,
            pEnd = (sbyte*)desc.Bytecode.NativePointer + desc.Bytecode.LongCount
        })).ThrowOnError("Couldn't initialize compute kernel");
    }

    [DllImport("UnCompute")]
    private static extern ResultCode IKernel_Init(IntPtr self, in DescNative desc);

    [DllImport("UnCompute")]
    private static extern void IKernel_GetDesc(IntPtr self, out DescNative desc);

    [StructLayout(LayoutKind.Sequential)]
    private readonly record struct DescNative(NativeString Name, IntPtr ResourceBinding, ArraySliceBase Bytecode);

    /// <summary>
    ///     Kernel descriptor.
    /// </summary>
    /// <param name="Name">Kernel debug name.</param>
    public readonly record struct Desc(NativeString Name, ResourceBinding ResourceBinding, NativeArray<byte> Bytecode);
}
