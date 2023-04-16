﻿using System.Globalization;
using UraniumCompute.Acceleration;
using UraniumCompute.Acceleration.Pipelines;
using UraniumCompute.Acceleration.TransientResources;
using UraniumCompute.Compiler.InterimStructs;

using var scheduler = JobScheduler.CreateForVulkan();
using var pipeline = scheduler.CreatePipeline();

var sourceData = Enumerable.Range(0, 1024).Select(x => (float)x).ToArray();

var bufferA = TransientBuffer1D<float>.Null;
var bufferB = TransientBuffer1D<float>.Null;
var bufferC = TransientBuffer1D<float>.Null;
var bufferD = TransientBuffer1D<float>.Null;

pipeline.AddHostJob("x",
    ctx => ctx.CreateBuffer(out bufferA, "Source buffer copy", (ulong)sourceData.Length, MemoryKindFlags.HostAndDeviceAccessible),
    () =>
    {
        using var map = bufferA.Buffer.Map();
        sourceData.CopyTo(map[..]);
    }
);
pipeline.AddDeviceJob("2x",
    ctx => ctx
        .SetWorkgroups(bufferA)
        .CreateBuffer(out bufferB, "BufferB", (ulong)sourceData.Length, MemoryKindFlags.DeviceAccessible)
        .ReadBuffer(bufferA),
    (Span<float> newBuffer, Span<float> sourceBuffer) =>
    {
        var index = (int)GpuIntrinsic.GetGlobalInvocationId().X;
        newBuffer[index] = sourceBuffer[index] * 2;
    }
);
pipeline.AddDeviceJob("Sin(2x)",
    ctx => ctx
        .SetWorkgroups(bufferA)
        .CreateBuffer(out bufferC, "Buffer C", (ulong)sourceData.Length, MemoryKindFlags.DeviceAccessible)
        .ReadBuffer(bufferB),
    (Span<float> newBuffer, Span<float> sourceBuffer) =>
    {
        var index = (int)GpuIntrinsic.GetGlobalInvocationId().X;
        newBuffer[index] = MathF.Sin(sourceBuffer[index]);
    }
);
pipeline.AddDeviceJob("Sin(2x) + 500",
    ctx => ctx
        .SetWorkgroups(bufferA)
        .CreateBuffer(out bufferD, "Buffer D", (ulong)sourceData.Length, MemoryKindFlags.HostAndDeviceAccessible)
        .ReadBuffer(bufferC),
    (Span<float> newBuffer, Span<float> sourceBuffer) =>
    {
        var index = (int)GpuIntrinsic.GetGlobalInvocationId().X;
        newBuffer[index] = sourceBuffer[index] + 500;
    }
);

Console.WriteLine("Waiting for tasks to complete...");
var result = await pipeline.Run();
Console.WriteLine($"Task was completed in {result.ElapsedMilliseconds}ms");

using (var map = bufferD.Buffer.Map())
{
    Console.WriteLine(
        $"Calculation results: [{string.Join(", ", map.Select(x => x.ToString(CultureInfo.InvariantCulture)).Take(32))}, ...]");
}