using UraniumCompute.Backend;

namespace UraniumCompute.Acceleration.Pipelines;

public interface IJobContext : IDisposable
{
    Pipeline Pipeline { get; }
    void Setup(out ulong requiredDeviceMemory, out ulong requiredHostMemory);
    void Init();
    void Run(ICommandRecordingContext ctx);
}
