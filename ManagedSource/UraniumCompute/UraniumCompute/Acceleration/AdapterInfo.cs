using System.Runtime.InteropServices;
using System.Text;

namespace UraniumCompute.Acceleration;

/// <summary>
///     Description of backend's hardware adapter.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct AdapterInfo
{
    /// <summary>
    ///     Maximum length of <see cref="AdapterInfo.Name" />.
    /// </summary>
    public const int MaxNameLength = 256;

    /// <summary>
    ///     Adapter ID, used to create a compute device on it.
    /// </summary>
    public readonly int Id;

    /// <summary>
    ///     Kind of adapter (integrated, discrete, etc.).
    /// </summary>
    public readonly AdapterKind Kind;

    /// <summary>
    ///     Name of adapter as a fixed array of <see cref="MaxNameLength" /> bytes, encoded in ASCII.
    /// </summary>
    /// <seealso cref="GetNameAsString()" />
    public unsafe fixed byte Name[MaxNameLength];

    /// <summary>
    ///     Get adapter's name as a managed string.
    /// </summary>
    /// <returns>The decoded managed string.</returns>
    public unsafe string GetNameAsString()
    {
        fixed (byte* ptr = Name)
        {
            var byteCount = new Span<byte>(ptr, 256).IndexOf((byte)0);
            return Encoding.ASCII.GetString(ptr, byteCount);
        }
    }

    public override string ToString()
    {
        return $"{Kind} Adapter [ID = {Id}] \"{GetNameAsString()}\"";
    }
}
