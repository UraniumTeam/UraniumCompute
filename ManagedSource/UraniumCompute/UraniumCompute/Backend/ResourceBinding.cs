using System.Runtime.InteropServices;
using UraniumCompute.Acceleration;
using UraniumCompute.Containers;
using UraniumCompute.Memory;

namespace UraniumCompute.Backend;

public sealed class ResourceBinding : DeviceObject<ResourceBinding.Desc>
{
    public override Desc Descriptor
    {
        get
        {
            IResourceBinding_GetDesc(Handle, out var descNative);
            return new Desc(descNative.Name, descNative.Layout.AsSpan<KernelResourceDesc>().ToArray());
        }
    }

    internal ResourceBinding(IntPtr handle) : base(handle)
    {
    }

    public override unsafe void Init(in Desc desc)
    {
        fixed (KernelResourceDesc* p = desc.Layout)
        {
            var descNative = new DescNative(desc.Name, new ArraySliceBase
            {
                pBegin = (sbyte*)p,
                pEnd = (sbyte*)(p + desc.Layout.Length)
            });

            IResourceBinding_Init(Handle, in descNative).ThrowOnError("Couldn't initialize device memory");
        }
    }

    public void SetVariable<T>(int bindingIndex, Buffer<T> buffer)
        where T : unmanaged
    {
        IResourceBinding_SetVariable(Handle, bindingIndex, buffer.Handle).ThrowOnError("Couldn't set kernel variable");
    }

    [DllImport("UnCompute")]
    private static extern ResultCode IResourceBinding_Init(IntPtr self, in DescNative desc);

    [DllImport("UnCompute")]
    private static extern void IResourceBinding_GetDesc(IntPtr self, out DescNative desc);

    [DllImport("UnCompute")]
    private static extern ResultCode IResourceBinding_SetVariable(IntPtr self, int bindingIndex, IntPtr buffer);

    [StructLayout(LayoutKind.Sequential)]
    private readonly record struct DescNative(NativeString Name, ArraySliceBase Layout);

    /// <summary>
    ///     Resource binding descriptor.
    /// </summary>
    /// <param name="Name">Resource binding debug name.</param>
    /// <param name="Layout">Array of kernel resource descriptors.</param>
    public readonly record struct Desc(NativeString Name, KernelResourceDesc[] Layout);
}
