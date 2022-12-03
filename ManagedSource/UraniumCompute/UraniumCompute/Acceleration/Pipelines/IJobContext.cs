namespace UraniumCompute.Acceleration.Pipelines;

public interface IJobContext : IDisposable
{
    Pipeline Pipeline { get; }
    void Init();
    void Run();
}
