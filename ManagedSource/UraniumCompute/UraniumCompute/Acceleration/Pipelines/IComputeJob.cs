namespace UraniumCompute.Acceleration.Pipelines;

public interface IComputeJob
{
    string Name { get; }

    void Run(IJobRunContext ctx);
}
