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
            return desc;
        }
    }

    internal Kernel(IntPtr handle) : base(handle)
    {
    }

    protected override void InitInternal(in Desc desc)
    {
        IKernel_Init(Handle, in desc).ThrowOnError("Couldn't initialize compute kernel");
    }

    [DllImport("UnCompute")]
    private static extern ResultCode IKernel_Init(IntPtr self, in Desc desc);

    [DllImport("UnCompute")]
    private static extern void IKernel_GetDesc(IntPtr self, out Desc desc);

    /// <summary>
    ///     Kernel descriptor.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Desc
    {
        /// <summary>
        ///     Kernel debug name.
        /// </summary>
        public NativeString Name { internal get; init; }

        /// <summary>
        ///     Resource binding object that binds resources for the kernel.
        /// </summary>
        public ResourceBinding ResourceBinding
        {
            init => resourceBinding = value.Handle;
        }

        /// <summary>
        ///     Kernel program bytecode.
        /// </summary>
        public ReadOnlySpan<byte> Bytecode
        {
            internal get => bytecode.AsSpan<byte>();
            init
            {
                unsafe
                {
                    fixed (byte* p = value)
                    {
                        bytecode = ArraySliceBase.Create(p, value.Length);
                    }
                }
            }
        }

        private readonly IntPtr resourceBinding;
        private readonly ArraySliceBase bytecode;

        /// <summary>
        ///     Kernel descriptor.
        /// </summary>
        /// <param name="name">Kernel debug name.</param>
        /// <param name="resourceBinding">Resource binding object that binds resources for the kernel.</param>
        /// <param name="bytecode">Kernel program bytecode.</param>
        public Desc(NativeString name, ResourceBinding resourceBinding, ReadOnlySpan<byte> bytecode)
        {
            Name = name;
            this.resourceBinding = resourceBinding.Handle;
            this.bytecode = new ArraySliceBase();
            Bytecode = bytecode;
        }
    }
}
