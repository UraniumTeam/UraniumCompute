namespace UraniumCompute.Backend;

/// <summary>
///     Kind of resource that is bound to a kernel.
/// </summary>
public enum KernelResourceKind
{
    /// <summary>
    ///     Read-only buffer.
    /// </summary>
    Buffer,

    /// <summary>
    ///     Constant buffer.
    /// </summary>
    ConstantBuffer,

    /// <summary>
    ///     Storage buffer with unordered access.
    /// </summary>
    RWBuffer,

    /// <summary>
    ///     Read-only sampled image.
    /// </summary>
    SampledTexture,

    /// <summary>
    ///     Storage image with unordered access.
    /// </summary>
    RWTexture,

    /// <summary>
    ///     Texture sampler.
    /// </summary>
    Sampler
}
