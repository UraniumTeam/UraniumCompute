using UraniumCompute.Acceleration.TransientResources;
using UraniumCompute.Memory;

namespace UraniumCompute.Acceleration.Pipelines;

public interface IJobSetupContext : IJobContext
{
    IJobSetupContext CreateBuffer<T>(out TransientBuffer1D<T> buffer, Buffer1D<T>.Desc desc, MemoryKindFlags memoryKindFlags)
        where T : unmanaged;

    IJobSetupContext ReadBuffer<T>(ITransientBuffer<T> buffer)
        where T : unmanaged;

    IJobSetupContext WriteBuffer<T>(ITransientBuffer<T> buffer)
        where T : unmanaged;
}

public static class JobContextExtensions
{
    public static IJobSetupContext CreateBuffer<T>(this IJobSetupContext ctx, out TransientBuffer1D<T> buffer, NativeString Name,
        ulong XDimension, MemoryKindFlags memoryKindFlags)
        where T : unmanaged
    {
        return ctx.CreateBuffer(out buffer, new Buffer1D<T>.Desc(Name, XDimension), memoryKindFlags);
    }
}
