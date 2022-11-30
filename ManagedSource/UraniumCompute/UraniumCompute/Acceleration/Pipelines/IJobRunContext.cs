namespace UraniumCompute.Acceleration.Pipelines;

public interface IJobRunContext
{
    void Run(Delegate jobDelegate);
}
