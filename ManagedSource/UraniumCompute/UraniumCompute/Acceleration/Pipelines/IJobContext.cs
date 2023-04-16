using UraniumCompute.Acceleration.TransientResources;
using UraniumCompute.Backend;

namespace UraniumCompute.Acceleration.Pipelines;

public interface IJobContext : IDisposable
{
    IComputeJob ComputeJob { get; }

    List<ITransientResource> CreatedResources { get; }
    List<ITransientResource> ReadResources { get; }
    List<ITransientResource> WrittenResources { get; }

    Pipeline Pipeline { get; }
    void Setup(out ulong requiredDeviceMemory, out ulong requiredHostMemory);
    void Init();
    void Run(ICommandRecordingContext ctx);
}
