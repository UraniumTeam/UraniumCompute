using UraniumCompute.Acceleration;
using UraniumCompute.Acceleration.Pipelines;
using UraniumCompute.Compiler.Decompiling;
using UraniumCompute.Compiler.InterimStructs;

namespace PipelinesSample;

public sealed class AddArraysJob : IDeviceJob
{
    public string Name => "Add two arrays on GPU";

    public Buffer1D<float> First { get; }
    public Buffer1D<float> Second { get; }
    public Buffer1D<float> Result => result;

    private Buffer1D<float> result;

    public AddArraysJob(Buffer1D<float> first, Buffer1D<float> second, JobScheduler scheduler)
    {
        First = first;
        Second = second;
        result = null!;
    }

    public IJobInitContext Init(IDeviceJobInitContext ctx)
    {
        return ctx
            .SetWorkgroups(1, 1, 1)
            .ReadBuffer(First)
            .ReadBuffer(Second)
            .CreateBuffer(out result, "Add arrays job result", First.Count, MemoryKindFlags.HostAndDeviceAccessible);
    }

    public void Run(IJobRunContext ctx)
    {
        ctx.Run(AddArraysOnGpu);
    }

    [Kernel(X = 1, Y = 1, Z = 1)]
    private static void AddArraysOnGpu(Span<float> a, Span<float> b, Span<float> result)
    {
        var index = (int)GpuIntrinsic.GetGlobalInvocationId().X;
        result[index] = a[index] + b[index];
    }
}
