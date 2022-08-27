using System.Runtime.InteropServices;
using System.Text;
using UraniumCompute.Containers;

namespace UraniumCompute.Memory;

[StructLayout(LayoutKind.Sequential)]
public readonly struct NativeString
{
    private readonly IntPtr nativePointer;
    private static readonly List<MemoryPage> charData = new();

    private unsafe NativeString(IntPtr nativePointer, byte[] data)
    {
        var length = data.Length;
        new Span<byte>(data).CopyTo(new Span<byte>((void*)nativePointer, length));
        this.nativePointer = nativePointer;
    }

    public static implicit operator NativeString(string s)
    {
        return CreateStatic(s);
    }

    public override unsafe string ToString()
    {
        var pointer = (byte*)nativePointer;
        var index = 0;
        for (;; ++index)
        {
            if (pointer[index] == 0)
            {
                break;
            }
        }

        return Encoding.ASCII.GetString(pointer, index);
    }

    public static NativeString CreateStatic(string s)
    {
        var chars = s.ToCharArray();
        var bytes = new byte[chars.Length + 1];
        for (var i = 0; i < chars.Length; ++i)
        {
            bytes[i] = unchecked((byte)chars[i]);
        }

        foreach (var memoryPage in charData)
        {
            var ptr = memoryPage.Allocate(bytes.Length);
            if (ptr != IntPtr.Zero)
            {
                return new NativeString(ptr, bytes);
            }
        }

        charData.Add(new MemoryPage(charData.LastOrDefault()?.Array.LongCount * 2 ?? bytes.Length));
        return new NativeString(charData.Last().Allocate(bytes.Length), bytes);
    }

    private class MemoryPage
    {
        public NativeArray<byte> Array;
        private long usedBytes;

        public MemoryPage(long size)
        {
            Array = new NativeArray<byte>(size);
            usedBytes = 0;
        }

        public IntPtr Allocate(long size)
        {
            if (Array.Count - usedBytes < size)
            {
                return IntPtr.Zero;
            }

            usedBytes += size;
            return Array.NativePointer + (int)(usedBytes - size);
        }
    }
}
