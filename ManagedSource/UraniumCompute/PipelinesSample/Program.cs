﻿using System.Diagnostics;
using System.Globalization;
using PipelinesSample;
using UraniumCompute.Acceleration;
using UraniumCompute.Acceleration.Pipelines;
using UraniumCompute.Acceleration.TransientResources;
using UraniumCompute.Compiler.InterimStructs;

using var scheduler = JobScheduler.CreateForVulkan();
using var pipeline = scheduler.CreatePipeline();
var bufferA = TransientBuffer1D<float>.Null;
var bufferB = TransientBuffer1D<float>.Null;

const int batchSize = 128;
pipeline.AddHostJob("Create buffer A",
    ctx => ctx.CreateBuffer(out bufferA, "buffer A", 1024 * 1024, MemoryKindFlags.HostAndDeviceAccessible),
    () =>
    {
        using var map = bufferA.Buffer.Map();
        var i = 0u;
        foreach (ref var x in map[..])
        {
            x = i++;
        }
    }
);
pipeline.AddDeviceJob("Transform buffer A",
    ctx => ctx.SetWorkgroups(bufferA, batchSize).Write(bufferA),
    (Span<float> a) => { a[(int)GpuIntrinsic.GetGlobalInvocationId().X] *= 2; }
);
pipeline.AddDeviceJob("Create buffer B",
    ctx => ctx.SetWorkgroups(bufferA, batchSize)
        .CreateBuffer(out bufferB, "buffer B", bufferA.LongCount, MemoryKindFlags.DeviceAccessible),
    (Span<float> a) => { a[(int)GpuIntrinsic.GetGlobalInvocationId().X] = GpuIntrinsic.GetGlobalInvocationId().X; }
);
var addAB = pipeline.AddDeviceJob(new AddArraysJob(bufferA, bufferB, batchSize));
var copy = pipeline.AddCopyJob("Copy to host", addAB.Result, MemoryKindFlags.HostAndDeviceAccessible);

Console.WriteLine("Waiting for tasks to complete...");
var result = await pipeline.Run();
Console.WriteLine($"Task was completed in {result.ElapsedMilliseconds}ms");

using (var map = copy.Destination.Buffer.Map())
{
    for (var i = 0; i < map.Count; i++)
    {
        Trace.Assert(Math.Abs(i + 2 * i - map[i]) < 0.0001f);
    }
    Console.WriteLine(
        $"Calculation results: [{string.Join(", ", map.Select(x => x.ToString(CultureInfo.InvariantCulture)).Take(32))}, ...]");
}
