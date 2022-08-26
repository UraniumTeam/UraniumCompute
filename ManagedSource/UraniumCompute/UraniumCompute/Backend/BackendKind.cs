namespace UraniumCompute.Backend;

/// <summary>
///     Kind of backend, either a kind of GPU API or CPU.
/// </summary>
public enum BackendKind
{
    /// <summary>
    ///     CPU backend, jobs on this kind of backend will be executed on the CPU.
    /// </summary>
    Cpu,

    /// <summary>
    ///     Vulkan backend, jobs on this kind of backend will be executed on the GPU
    ///     using Vulkan API compute shaders.
    /// </summary>
    Vulkan
}
