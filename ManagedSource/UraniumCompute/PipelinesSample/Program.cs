using System.Globalization;
using PipelinesSample;
using UraniumCompute.Acceleration;
using UraniumCompute.Acceleration.Pipelines;
using UraniumCompute.Compiler.InterimStructs;

using var scheduler = JobScheduler.CreateForVulkan();
using var pipeline = scheduler.CreatePipeline();
var bufferA = (Buffer1D<float>)null!;
var bufferB = (Buffer1D<float>)null!;

var createA = pipeline.AddHostJob("Create buffer A",
    ctx => ctx.CreateBuffer(out bufferA, "buffer A", 1024, MemoryKindFlags.HostAndDeviceAccessible),
    () =>
    {
        using var map = bufferA.Map();
        var i = 0u;
        foreach (ref var x in map[..])
        {
            x = i++;
        }
    }
);
var convertA = pipeline.AddDeviceJob("Transform buffer A",
    ctx => ctx.WriteBuffer(bufferA),
    (Span<float> a) => a[(int)GpuIntrinsic.GetGlobalInvocationId().X] *= 2
);
var createB = pipeline.AddDeviceJob("Create buffer B",
    ctx => ctx.CreateBuffer(out bufferB, "buffer B", 1024, MemoryKindFlags.DeviceAccessible),
    (Span<float> a) => a[(int)GpuIntrinsic.GetGlobalInvocationId().X] = GpuIntrinsic.GetGlobalInvocationId().X
);
var addAB = pipeline.AddDeviceJob(new AddArraysJob(bufferA, bufferB, scheduler));

await pipeline.Run();

using (var map = addAB.Result.Map())
{
    Console.WriteLine(
        $"Calculation results: [{string.Join(", ", map.Select(x => x.ToString(CultureInfo.InvariantCulture)).Take(32))}, ...]");
}
