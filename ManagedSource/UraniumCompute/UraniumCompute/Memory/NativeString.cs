using System.Runtime.InteropServices;
using System.Text;
using UraniumCompute.Containers;

namespace UraniumCompute.Memory;

[StructLayout(LayoutKind.Sequential)]
public readonly struct NativeString
{
    private readonly unsafe byte* nativePointer;
    private static readonly List<MemoryPage> charData = new();

    private unsafe NativeString(byte* nativePointer, char* data, int length)
    {
        this.nativePointer = nativePointer;
        for (var i = 0; i < length; ++i)
        {
            nativePointer[i] = (byte)data[i];
        }
    }

    public static implicit operator NativeString(string s)
    {
        return CreateStatic(s);
    }

    public unsafe ReadOnlySpan<byte> AsSpan()
    {
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

    public static unsafe NativeString CreateStatic(string s)
    {
        var length = s.Length;
        fixed (char* chars = s)
        {
            var bytes = AllocateBytes(length + 1);
            bytes[length] = 0;
            return new NativeString(bytes, chars, length);
        }
    }

    private static unsafe byte* AllocateBytes(long length)
    {
        foreach (var memoryPage in charData)
        {
            var ptr = memoryPage.Allocate(length);
            if (ptr != null)
            {
                return ptr;
            }
        }

        charData.Add(new MemoryPage(charData.LastOrDefault()?.Array.LongCount * 2 ?? length));
        return charData.Last().Allocate(length);
    }

    private unsafe class MemoryPage
    {
        public NativeArray<byte> Array;
        private long usedBytes;

        public MemoryPage(long size)
        {
            Array = new NativeArray<byte>(size);
            usedBytes = 0;
        }

        public byte* Allocate(long size)
        {
            if (Array.Count - usedBytes < size)
            {
                return null;
            }

            usedBytes += size;
            return (byte*)Array.NativePointer + (usedBytes - size);
        }
    }
}
