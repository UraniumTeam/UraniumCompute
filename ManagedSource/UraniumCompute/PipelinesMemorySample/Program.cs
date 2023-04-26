using System.Diagnostics;
using System.Globalization;
using UraniumCompute.Acceleration;
using UraniumCompute.Acceleration.Pipelines;
using UraniumCompute.Acceleration.TransientResources;
using UraniumCompute.Compiler.InterimStructs;

using var scheduler = JobScheduler.CreateForVulkan();
using var pipeline = scheduler.CreatePipeline();

const int workgroupSize = 128;

var sourceData = Enumerable.Range(0, 2 * 1024 * 1024).Select(x => (float)(x % 16)).ToArray();

var bufferA = TransientBuffer1D<float>.Null;
var bufferB = TransientBuffer1D<float>.Null;
var bufferC = TransientBuffer1D<float>.Null;
var bufferD = TransientBuffer1D<float>.Null;
var bufferE = TransientBuffer1D<float>.Null;
var bufferF = TransientBuffer1D<float>.Null;
var bufferG = TransientBuffer1D<float>.Null;

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
        .SetWorkgroups(bufferA, workgroupSize)
        .CreateBuffer(out bufferB, "BufferB", (ulong)sourceData.Length, MemoryKindFlags.DeviceAccessible)
        .ReadBuffer(bufferA),
    (Span<float> newBuffer, Span<float> sourceBuffer) =>
    {
        var index = (int)GpuIntrinsic.GetGlobalInvocationId().X;
        newBuffer[index] = sourceBuffer[index] * 2;
    }
);
pipeline.AddDeviceJob("3x",
    ctx => ctx
        .SetWorkgroups(bufferA, workgroupSize)
        .CreateBuffer(out bufferC, "BufferC", (ulong)sourceData.Length, MemoryKindFlags.DeviceAccessible)
        .ReadBuffer(bufferA),
    (Span<float> newBuffer, Span<float> sourceBuffer) =>
    {
        var index = (int)GpuIntrinsic.GetGlobalInvocationId().X;
        newBuffer[index] = sourceBuffer[index] * 3;
    }
);
pipeline.AddDeviceJob("2x + 100",
    ctx => ctx
        .SetWorkgroups(bufferA, workgroupSize)
        .CreateBuffer(out bufferD, "BufferD", (ulong)sourceData.Length, MemoryKindFlags.DeviceAccessible)
        .ReadBuffer(bufferB),
    (Span<float> newBuffer, Span<float> sourceBuffer) =>
    {
        var index = (int)GpuIntrinsic.GetGlobalInvocationId().X;
        newBuffer[index] = sourceBuffer[index] + 100;
    }
);
pipeline.AddDeviceJob("cos(3x)",
    ctx => ctx
        .SetWorkgroups(bufferA, workgroupSize)
        .CreateBuffer(out bufferF, "BufferF", (ulong)sourceData.Length, MemoryKindFlags.DeviceAccessible)
        .ReadBuffer(bufferC),
    (Span<float> newBuffer, Span<float> sourceBuffer) =>
    {
        var index = (int)GpuIntrinsic.GetGlobalInvocationId().X;
        newBuffer[index] = MathF.Cos(sourceBuffer[index]);
    }
);
pipeline.AddDeviceJob("sin(2x + 100)",
    ctx => ctx
        .SetWorkgroups(bufferA, workgroupSize)
        .CreateBuffer(out bufferE, "BufferE", (ulong)sourceData.Length, MemoryKindFlags.DeviceAccessible)
        .ReadBuffer(bufferD),
    (Span<float> newBuffer, Span<float> sourceBuffer) =>
    {
        var index = (int)GpuIntrinsic.GetGlobalInvocationId().X;
        newBuffer[index] = MathF.Sin(sourceBuffer[index]);
    }
);
pipeline.AddDeviceJob("sin(2x + 100) + cos(3x) + 500",
    ctx => ctx
        .SetWorkgroups(bufferA, workgroupSize)
        .CreateBuffer(out bufferG, "BufferG", (ulong)sourceData.Length, MemoryKindFlags.HostAndDeviceAccessible)
        .ReadBuffer(bufferE)
        .ReadBuffer(bufferF),
    (Span<float> newBuffer, Span<float> sourceE, Span<float> sourceF) =>
    {
        var index = (int)GpuIntrinsic.GetGlobalInvocationId().X;
        newBuffer[index] = sourceE[index] + sourceF[index] + 500;
    }
);

Console.WriteLine("Waiting for tasks to complete on GPU...");
var result = await pipeline.Run();
Console.WriteLine($"Task was completed in {result.ElapsedMilliseconds}ms");

var sw = new Stopwatch();
var cpuResult = new float[sourceData.Length];
Console.WriteLine("Waiting for tasks to complete on CPU...");
sw.Start();
for (var i = 0; i < cpuResult.Length; ++i)
{
    var x = sourceData[i];
    cpuResult[i] = MathF.Sin(2 * x + 100) + MathF.Cos(3 * x) + 500;
}
sw.Stop();
Console.WriteLine($"Task was completed in {sw.ElapsedMilliseconds}ms");

using (var map = bufferG.Buffer.Map())
{
    Console.WriteLine(
        $"Calculation results: [{string.Join(", ", map.Select(x => x.ToString(CultureInfo.InvariantCulture)).Take(32))}, ...]");

    for (var i = 0; i < sourceData.Length; i++)
    {
        Trace.Assert(Math.Abs(map[i] - cpuResult[i]) < 0.0001f, $"index was {i}");
    }
}
