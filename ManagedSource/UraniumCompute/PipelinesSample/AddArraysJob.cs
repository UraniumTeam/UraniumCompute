using UraniumCompute.Acceleration;
using UraniumCompute.Acceleration.Pipelines;
using UraniumCompute.Acceleration.TransientResources;
using UraniumCompute.Compiler.Decompiling;
using UraniumCompute.Compiler.InterimStructs;

namespace PipelinesSample;

public sealed class AddArraysJob : IDeviceJob
{
    public string Name => "Add two arrays on GPU";

    public TransientBuffer1D<float> First { get; }
    public TransientBuffer1D<float> Second { get; }
    public TransientBuffer1D<float> Result => result;

    private TransientBuffer1D<float> result;

    public AddArraysJob(TransientBuffer1D<float> first, TransientBuffer1D<float> second)
    {
        First = first;
        Second = second;
        result = TransientBuffer1D<float>.Null;
    }

    public IJobSetupContext Setup(IDeviceJobSetupContext ctx)
    {
        return ctx
            .SetWorkgroups(First)
            .ReadBuffer(First)
            .ReadBuffer(Second)
            .CreateBuffer(out result, "Add arrays job result", First.LongCount, MemoryKindFlags.HostAndDeviceAccessible);
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
