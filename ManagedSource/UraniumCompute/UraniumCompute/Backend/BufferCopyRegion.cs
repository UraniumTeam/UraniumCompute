using System.Runtime.InteropServices;

namespace UraniumCompute.Backend;

[StructLayout(LayoutKind.Sequential)]
public readonly record struct BufferCopyRegion(ulong Size, uint SourceOffset = 0, uint DestOffset = 0);
