namespace UraniumCompute.Acceleration.Pipelines;

public interface IJobRunContext : IJobContext
{
    void Run(Delegate jobDelegate);
}
