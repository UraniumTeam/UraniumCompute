# UraniumCompute

![Actions Status](https://github.com/UraniumTeam/UraniumCompute/workflows/Build/badge.svg)

UraniumCompute is a C++ and C# library for GPU-accelerated computing. It is built on top of Vulkan API, so you don't need a CUDA-enabled GPU to use the library.
Your C# code is compiled to SPIR-V bytecode at runtime and is executed as a Vulkan compute shader. All buffer descriptors are managed automatically.

## Building
Use CMake to build C++ projects and .Net 7.0 to build C# projects.

See our [build instructions](./BUILDING.md) for more info.

## Usage
UraniumCompute has two different APIs - a high-level API and a low-level API.
The high-level API is implemented as a GPU job graph that automatically allocates device memory efficiently for buffers and synchronizes access to device
objects by placing memory barriers and fences.

Here's an example in C#
```cs
// First we create a job scheduler that holds Vulkan instance and devices, you need only one scheduler in your application
using var scheduler = JobScheduler.CreateForVulkan();
// Then we create a pipeline that manages device and host jobs to be executed
using var pipeline = scheduler.CreatePipeline();
// We declare transient buffers, that will be allocated lazily on the first use
var bufferA = TransientBuffer1D<float>.Null;
var bufferB = TransientBuffer1D<float>.Null;

// Now we add a host (CPU) job that creates a 256kB buffer and fills it with data
// The code won't be exeucted right now, we only schedule the jobs to the pipeline
pipeline.AddHostJob("Create buffer A",
    ctx => ctx.CreateBuffer(out bufferA, "buffer A", 256 * 1024, MemoryKindFlags.HostAndDeviceAccessible),
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
// Then we add three device (GPU) jobs to the pipeline, they will run the actual computation code
pipeline.AddDeviceJob("Transform buffer A",
    ctx => ctx.SetWorkgroups(bufferA).WriteBuffer(bufferA),
    (Span<float> a) => { a[(int)GpuIntrinsic.GetGlobalInvocationId().X] *= 2; } // This code will be compiled to SPIR-V
);
pipeline.AddDeviceJob("Create buffer B",
    ctx => ctx.SetWorkgroups(bufferA.Count)
        .CreateBuffer(out bufferB, "buffer B", bufferA.LongCount, MemoryKindFlags.DeviceAccessible),
    (Span<float> a) => { a[(int)GpuIntrinsic.GetGlobalInvocationId().X] = GpuIntrinsic.GetGlobalInvocationId().X; }
);
// The jobs can also be create with classes instead of lambdas, this allows us to store frequently used code separately
var addAB = pipeline.AddDeviceJob(new AddArraysJob(bufferA, bufferB));
```
