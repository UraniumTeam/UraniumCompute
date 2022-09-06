using System.Runtime.InteropServices;

namespace UraniumCompute.Backend;

/// <summary>
///     Region for buffer copy command.
/// </summary>
/// <param name="Size">Size of the copy region.</param>
/// <param name="SourceOffset">Offset in the source buffer.</param>
/// <param name="DestOffset">Offset in the destination buffer.</param>
[StructLayout(LayoutKind.Sequential)]
public readonly record struct BufferCopyRegion(ulong Size, uint SourceOffset = 0, uint DestOffset = 0);
