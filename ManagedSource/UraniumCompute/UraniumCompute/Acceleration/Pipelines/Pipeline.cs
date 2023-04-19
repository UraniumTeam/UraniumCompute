using System.Diagnostics;
using UraniumCompute.Acceleration.TransientResources;
using UraniumCompute.Backend;

namespace UraniumCompute.Acceleration.Pipelines;

public sealed class Pipeline : IDisposable
{
    public JobScheduler JobScheduler { get; }
    public bool IsInitialized { get; private set; }

    public int GCLatency { get; set; }

    private readonly List<ITransientResource> resources = new();
    private readonly List<IJobContext> jobs = new();

    private readonly TransientResourceHeap deviceHeap;
    private readonly TransientResourceHeap hostHeap;
    private ulong requiredHostMemory;
    private ulong requiredDeviceMemory;

    private readonly CommandList commandList;

    internal Pipeline(JobScheduler scheduler)
    {
        JobScheduler = scheduler;
        deviceHeap = new TransientResourceHeap(JobScheduler.Device);
        hostHeap = new TransientResourceHeap(JobScheduler.Device);
        commandList = JobScheduler.Device.CreateCommandList();
    }

    public T AddHostJob<T>(T job)
        where T : IHostJob
    {
        if (IsInitialized)
        {
            throw new InvalidOperationException("Pipeline was already initialized");
        }

        jobs.Add(new HostJobContext(job, this));
        jobs.Last().Setup(out var d, out var h);
        requiredHostMemory += h;
        requiredDeviceMemory += d;
        return job;
    }

    public T AddDeviceJob<T>(T job)
        where T : IDeviceJob
    {
        if (IsInitialized)
        {
            throw new InvalidOperationException("Pipeline was already initialized");
        }

        jobs.Add(new DeviceJobContext(job, this));
        jobs.Last().Setup(out var d, out var h);
        requiredHostMemory += h;
        requiredDeviceMemory += d;
        return job;
    }

    public void AddHostJob(string name, Func<IHostJobSetupContext, IJobSetupContext> initializer, Action jobDelegate)
    {
        _ = AddHostJob(new DelegateHostJob(name, initializer, jobDelegate));
    }

    public void AddDeviceJob(string name, Func<IDeviceJobSetupContext, IJobSetupContext> initializer, Delegate kernel)
    {
        _ = AddDeviceJob(new DelegateDeviceJob(name, initializer, kernel));
    }

    public async Task<Result> Run()
    {
        if (!IsInitialized)
        {
            if (requiredHostMemory > 0)
            {
                hostHeap.Init(MemoryKindFlags.HostAndDeviceAccessible, GCLatency, requiredHostMemory);
            }

            if (requiredDeviceMemory > 0)
            {
                deviceHeap.Init(MemoryKindFlags.DeviceAccessible, GCLatency, requiredDeviceMemory);
            }

            foreach (var jobContext in jobs)
            {
                jobContext.Init();
                hostHeap.Allocator.GarbageCollect();
                deviceHeap.Allocator.GarbageCollect();

                Console.WriteLine($"Host   heap: {hostHeap.Allocator.AllocatedByteCount}");
                Console.WriteLine($"Device heap: {deviceHeap.Allocator.AllocatedByteCount}");
            }

            commandList.Init(new CommandList.Desc("Command list", HardwareQueueKindFlags.Compute));
            IsInitialized = true;
        }

        using (var ctx = commandList.Begin())
        {
            foreach (var jobContext in jobs)
            {
                jobContext.Run(ctx);
            }
        }

        commandList.Submit();
        var sw = new Stopwatch();
        sw.Start();
        await Task.Run(() => commandList.CompletionFence.WaitOnCpu());
        sw.Stop();
        return new Result(sw.ElapsedMilliseconds);
    }

    public void Dispose()
    {
        deviceHeap.Dispose();
        hostHeap.Dispose();
        foreach (var jobContext in jobs)
        {
            jobContext.Dispose();
        }
    }

    internal void AddResource(ITransientResource resource)
    {
        resources.Add(resource);
        resource.Id = resources.Count - 1;
    }

    internal BufferBase InitResource(int id, BufferBase resource)
    {
        resources[id].Resource = resource;
        return resource;
    }

    internal BufferBase GetResource(int id)
    {
        return resources[id].Resource ?? throw new ArgumentException($"Resource {id} was uninitialized");
    }

    public TransientResourceHeap GetTransientResourceHeap(MemoryKindFlags memoryKindFlags)
    {
        return memoryKindFlags.HasFlag(MemoryKindFlags.HostAccessible) ? hostHeap : deviceHeap;
    }

    public record struct Result(long ElapsedMilliseconds);
}
