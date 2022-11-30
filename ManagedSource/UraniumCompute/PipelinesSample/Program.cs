using System.Globalization;
using PipelinesSample;
using UraniumCompute.Acceleration;
using UraniumCompute.Acceleration.Pipelines;
using UraniumCompute.Compiler.InterimStructs;

using var scheduler = JobScheduler.CreateForVulkan();
using var pipeline = scheduler.CreatePipeline();
using var bufferA = scheduler.CreateBuffer1D<float>();
using var bufferB = scheduler.CreateBuffer1D<float>();

var createA = pipeline.AddHostJob($"Create {bufferA.DebugName}",
    ctx => ctx.InitBuffer(bufferA, 1024, MemoryKindFlags.HostAndDeviceAccessible),
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
var convertA = pipeline.AddDeviceJob($"Transform {bufferA.DebugName}",
    ctx => ctx.WriteBuffer(bufferA),
    (Span<float> a) => a[(int)GpuIntrinsic.GetGlobalInvocationId().X] *= 2
);
var createB = pipeline.AddDeviceJob($"Create {bufferB.DebugName}",
    ctx => ctx.InitBuffer(bufferB, bufferA.Count, MemoryKindFlags.DeviceAccessible),
    (Span<float> a) => a[(int)GpuIntrinsic.GetGlobalInvocationId().X] = GpuIntrinsic.GetGlobalInvocationId().X
);
var addAB = pipeline.AddDeviceJob(new AddArraysJob(bufferA, bufferB, scheduler));

await pipeline.Run();

using (var map = addAB.Result.Map())
{
    Console.WriteLine(
        $"Calculation results: [{string.Join(", ", map.Select(x => x.ToString(CultureInfo.InvariantCulture)).Take(32))}, ...]");
}
