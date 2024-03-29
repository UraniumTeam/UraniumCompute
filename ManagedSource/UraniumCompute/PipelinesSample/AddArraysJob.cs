﻿using UraniumCompute.Acceleration;
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

    private TransientBuffer1D<float> result = null!;

    private readonly int workgroupSize;

    public AddArraysJob(TransientBuffer1D<float> first, TransientBuffer1D<float> second, int workgroupSize)
    {
        First = first;
        Second = second;
        this.workgroupSize = workgroupSize;
    }

    public IJobSetupContext Setup(IDeviceJobSetupContext ctx)
    {
        return ctx
            .SetWorkgroups(First, workgroupSize)
            .Read(First)
            .Read(Second)
            .CreateBuffer(out result, "Add arrays job result", First.LongCount, MemoryKindFlags.DeviceAccessible);
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
