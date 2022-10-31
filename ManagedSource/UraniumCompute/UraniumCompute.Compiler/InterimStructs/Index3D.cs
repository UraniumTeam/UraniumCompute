using System.Runtime.InteropServices;

namespace UraniumCompute.Compiler.InterimStructs;

[StructLayout(LayoutKind.Sequential)]
public readonly struct Index3D
{
    public int X { get; }
    public int Y { get; }
    public int Z { get; }

    public Index3D(int x, int y, int z)
    {
        X = x;
        Y = y;
        Z = z;
    }
}