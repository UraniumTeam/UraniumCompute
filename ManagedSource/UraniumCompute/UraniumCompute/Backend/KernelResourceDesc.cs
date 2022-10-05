using System.Runtime.InteropServices;

namespace UraniumCompute.Backend;

/// <summary>
///     Kernel resource descriptor.
/// </summary>
/// <param name="BindingIndex">Binding index in the compute shader source.</param>
/// <param name="Kind">Kind of resource that is bound to a kernel.</param>
[StructLayout(LayoutKind.Sequential)]
public readonly record struct KernelResourceDesc(int BindingIndex, KernelResourceKind Kind);
