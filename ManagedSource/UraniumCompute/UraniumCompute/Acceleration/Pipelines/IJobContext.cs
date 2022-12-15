using UraniumCompute.Acceleration.TransientResources;
using UraniumCompute.Backend;

namespace UraniumCompute.Acceleration.Pipelines;

public interface IJobContext : IDisposable
{
    public List<ITransientResource> CreatedResources { get; }
    public List<ITransientResource> ReadResources { get; }
    public List<ITransientResource> WrittenResources { get; }

    Pipeline Pipeline { get; }
    void Setup(out ulong requiredDeviceMemory, out ulong requiredHostMemory);
    void Init();
    void Run(ICommandRecordingContext ctx);
}
