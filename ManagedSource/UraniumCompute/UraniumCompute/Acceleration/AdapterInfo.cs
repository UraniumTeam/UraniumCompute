using System.Runtime.InteropServices;
using System.Text;

namespace UraniumCompute.Acceleration;

[StructLayout(LayoutKind.Sequential)]
public struct AdapterInfo
{
    public readonly int Id;
    public readonly AdapterKind Kind;
    private unsafe fixed byte name[256];

    public unsafe string Name
    {
        get
        {
            fixed (byte* ptr = name)
            {
                var byteCount = new Span<byte>(ptr, 256).IndexOf((byte)0);
                return Encoding.ASCII.GetString(ptr, byteCount);
            }
        }
    }

    public override string ToString()
    {
        return $"{Kind} Adapter [ID = {Id}] \"{Name}\"";
    }
}
