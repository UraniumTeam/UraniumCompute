using UraniumCompute.Memory;

namespace UraniumCompute.Backend;

/// <summary>
///     Base interface for device object descriptors.
/// </summary>
public interface IDeviceObjectDescriptor
{
    /// <summary>
    ///     Debug name of the object.
    /// </summary>
    NativeString Name { get; }
}
