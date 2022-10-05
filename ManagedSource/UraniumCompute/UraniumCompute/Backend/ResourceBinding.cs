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
            IResourceBinding_GetDesc(Handle, out var desc);
            return desc;
        }
    }

    internal ResourceBinding(IntPtr handle) : base(handle)
    {
    }

    public override void Init(in Desc desc)
    {
        IResourceBinding_Init(Handle, in desc).ThrowOnError("Couldn't initialize device memory");
    }

    public void SetVariable<T>(int bindingIndex, Buffer<T> buffer)
        where T : unmanaged
    {
        IResourceBinding_SetVariable(Handle, bindingIndex, buffer.Handle).ThrowOnError("Couldn't set kernel variable");
    }

    [DllImport("UnCompute")]
    private static extern ResultCode IResourceBinding_Init(IntPtr self, in Desc desc);

    [DllImport("UnCompute")]
    private static extern void IResourceBinding_GetDesc(IntPtr self, out Desc desc);

    [DllImport("UnCompute")]
    private static extern ResultCode IResourceBinding_SetVariable(IntPtr self, int bindingIndex, IntPtr buffer);

    /// <summary>
    ///     Resource binding descriptor.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Desc
    {
        /// <summary>Resource binding debug name.</summary>
        public NativeString Name { get; init; }

        /// <summary>Array of kernel resource descriptors.</summary>
        public ReadOnlySpan<KernelResourceDesc> Layout
        {
            internal get => layout.AsSpan<KernelResourceDesc>();
            init
            {
                unsafe
                {
                    fixed (KernelResourceDesc* p = value)
                    {
                        layout = ArraySliceBase.Create(p, value.Length);
                    }
                }
            }
        }

        private readonly ArraySliceBase layout;

        /// <summary>
        ///     Resource binding descriptor.
        /// </summary>
        /// <param name="name">Resource binding debug name.</param>
        /// <param name="layout">Array of kernel resource descriptors.</param>
        public Desc(NativeString name, ReadOnlySpan<KernelResourceDesc> layout)
        {
            Name = name;
            this.layout = new ArraySliceBase();
            Layout = layout;
        }
    }
}
