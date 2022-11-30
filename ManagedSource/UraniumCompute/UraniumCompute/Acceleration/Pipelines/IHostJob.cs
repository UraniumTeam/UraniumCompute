namespace UraniumCompute.Acceleration.Pipelines;

public interface IHostJob : IComputeJob
{
    IJobInitContext Init(IHostJobInitContext ctx);
}
