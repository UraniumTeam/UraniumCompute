using UraniumCompute.Backend;

namespace UraniumCompute.Acceleration.TransientResources;

public interface ITransientResource
{
    int Id { get; }
    BufferBase Resource { get; }
}
