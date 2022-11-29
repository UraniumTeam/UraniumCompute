using System.Runtime.InteropServices;
using System.Text;
using UraniumCompute.Containers;

namespace UraniumCompute.Memory;

/// <summary>
///     This struct can only be used to allocate temporary unmanaged strings to use them as unmanaged method arguments.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct NativeString
{
    private const int maxSize = 128;

    private readonly unsafe byte* nativePointer;
    private static readonly List<NativeArray<byte>> charData = new(maxSize);
    private static int currentIndex;

    private unsafe NativeString(byte* nativePointer, char* data, int length)
    {
        this.nativePointer = nativePointer;
        for (var i = 0; i < length; ++i)
        {
            nativePointer[i] = (byte)data[i];
        }

        nativePointer[length] = 0;
    }

    /// <summary>
    ///     Create from a managed string.
    /// </summary>
    /// <param name="s">Managed string.</param>
    /// <returns>A copy of the string in unmanaged memory.</returns>
    public static unsafe implicit operator NativeString(string s)
    {
        var length = s.Length;
        fixed (char* chars = s)
        {
            var bytes = AllocateBytes(length + 1);
            return new NativeString(bytes, chars, length);
        }
    }

    /// <summary>
    ///     Convert to a <see cref="ReadOnlySpan{T}" />.
    /// </summary>
    /// <returns>The created <see cref="ReadOnlySpan{T}" />.</returns>
    public unsafe ReadOnlySpan<byte> AsSpan()
    {
        if (nativePointer == null)
        {
            return ReadOnlySpan<byte>.Empty;
        }

        for (var i = 0;; ++i)
        {
            if (nativePointer[i] == 0)
            {
                return new ReadOnlySpan<byte>(nativePointer, i);
            }
        }
    }

    public override string ToString()
    {
        return Encoding.ASCII.GetString(AsSpan());
    }

    private static unsafe byte* AllocateBytes(long length)
    {
        if (charData.Count <= currentIndex)
        {
            charData.Add(new NativeArray<byte>(length));
        }
        else if (charData[currentIndex].Count < length)
        {
            charData[currentIndex].Dispose();
            charData[currentIndex] = new NativeArray<byte>(length);
        }

        var result = charData[currentIndex].NativePointer;
        currentIndex = (currentIndex + 1) % maxSize;
        return result;
    }
}
