using UraniumCompute.Acceleration.TransientResources;
using UraniumCompute.Common.Math;
using UraniumCompute.Memory;

namespace UraniumCompute.Acceleration.Pipelines;

public static class JobContextExtensions
{
    public static IJobSetupContext CreateBuffer<T>(this IJobSetupContext ctx, out TransientBuffer1D<T> buffer, NativeString Name,
        ulong XDimension, MemoryKindFlags memoryKindFlags)
        where T : unmanaged
    {
        return ctx.CreateBuffer(out buffer, new Buffer1D<T>.Desc(Name, XDimension), memoryKindFlags);
    }

    public static IDeviceJobSetupContext SetWorkgroups(this IDeviceJobSetupContext ctx, int x, int y = 1, int z = 1)
    {
        return ctx.SetWorkgroups(new Vector3Int(x, y, z));
    }

    public static IDeviceJobSetupContext SetWorkgroups<T>(this IDeviceJobSetupContext ctx, TransientBuffer1D<T> buffer,
        int workgroupSize = 1)
        where T : unmanaged
    {
        return ctx.SetWorkgroups(buffer.Count / workgroupSize);
    }

    public static IDeviceJobSetupContext SetWorkgroups<T>(this IDeviceJobSetupContext ctx, TransientBuffer2D<T> buffer)
        where T : unmanaged
    {
        return ctx.SetWorkgroups(buffer.Width, buffer.Height);
    }
}
