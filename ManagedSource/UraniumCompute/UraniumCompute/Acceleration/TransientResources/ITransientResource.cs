using UraniumCompute.Acceleration.Pipelines;
using UraniumCompute.Backend;

namespace UraniumCompute.Acceleration.TransientResources;

public interface ITransientResource
{
    int Id { get; set; }
    BufferBase Resource { get; set; }

    ICollection<IComputeJob> Readers { get; }
    ICollection<IComputeJob> Writers { get; }

    IComputeJob Creator { get; set; }
    IComputeJob Deleter { get; set; }

    MemoryKindFlags MemoryKindFlags { get; }
}
