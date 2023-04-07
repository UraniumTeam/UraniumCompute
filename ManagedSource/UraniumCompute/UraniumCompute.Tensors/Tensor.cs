using System.Diagnostics;
using System.Text;
using UraniumCompute.Acceleration;
using UraniumCompute.Backend;

namespace UraniumCompute.Tensors;

public abstract class Tensor
{
    public unsafe struct Shape
    {
        public int Count { get; }

        public ulong FlatLength
        {
            get
            {
                var result = 1UL;
                for (var i = 0; i < Count; i++)
                {
                    Debug.Assert(dimensions[i] > 0);
                    result *= (ulong)dimensions[i];
                }

                return result;
            }
        }

        private fixed long dimensions[8];

        private Shape(int count)
        {
            Count = count;
        }

        public static Shape Create(long d0)
        {
            var result = new Shape(1);
            result.dimensions[0] = d0;
            return result;
        }

        public static Shape Create(long d0, long d1)
        {
            var result = new Shape(2);
            result.dimensions[0] = d0;
            result.dimensions[1] = d1;
            return result;
        }

        public static Shape Create(long d0, long d1, long d2)
        {
            var result = new Shape(3);
            result.dimensions[0] = d0;
            result.dimensions[1] = d1;
            result.dimensions[2] = d2;
            return result;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("{ ");
            for (var i = 0; i < Count; i++)
            {
                sb.Append(dimensions[i]);
                sb.Append(' ');
            }

            sb.Append('}');
            return sb.ToString();
        }
    }
}

public sealed class Tensor<T> : IDisposable
    where T : unmanaged
{
    public Tensor.Shape Shape { get; private set; }

    public Span<T> HostStorage => hostStorage ?? throw new NullReferenceException("The host storage was null");

    private DeviceMemory? deviceMemory;
    private Buffer1D<T>? deviceStorage;
    private T[]? hostStorage;

    private Tensor(Tensor.Shape shape, T[]? hostStorage)
    {
        Shape = shape;
        this.hostStorage = hostStorage;
    }

    public static Tensor<T> CreateOnHost(Tensor.Shape shape)
    {
        return new Tensor<T>(shape, new T[shape.FlatLength]);
    }

    public static async Task<Tensor<T>> CreateOnDevice(ComputeDevice device, Tensor.Shape shape)
    {
        var result = CreateOnHost(shape);
        await result.ToDevice(device);
        return result;
    }

    public async Task ToHost()
    {
        if (deviceStorage is null)
        {
            throw new NullReferenceException("The device storage was null");
        }

        var device = deviceStorage.Device;

        using var hostBuffer = device.CreateBuffer1D<T>();
        hostBuffer.Init("Host buffer", Shape.FlatLength);
        using var hostMemory = hostBuffer.AllocateMemory("Host memory", MemoryKindFlags.HostAndDeviceAccessible);
        hostBuffer.BindMemory(hostMemory);

        using var commandList = device.CreateCommandList();
        commandList.Init(new CommandList.Desc("Command list", HardwareQueueKindFlags.Compute));

        using (var cmd = commandList.Begin())
        {
            cmd.MemoryBarrier(deviceStorage, AccessFlags.KernelWrite, AccessFlags.TransferRead);
            cmd.Copy(deviceStorage, hostBuffer);
            cmd.MemoryBarrier(hostBuffer, AccessFlags.TransferWrite, AccessFlags.HostRead);
        }

        commandList.Submit();
        await Task.Run(() => commandList.CompletionFence.WaitOnCpu());

        hostStorage = new T[Shape.FlatLength];

        using (var map = hostBuffer.Map())
        {
            map[..].CopyTo(hostStorage);
        }

        deviceMemory!.Dispose();
        deviceStorage.Dispose();
        deviceMemory = null;
        deviceStorage = null;
    }

    public async Task ToDevice(ComputeDevice device)
    {
        if (hostStorage is null)
        {
            throw new NullReferenceException("The host storage was null");
        }

        using var hostBuffer = device.CreateBuffer1D<T>();
        hostBuffer.Init("Host buffer", Shape.FlatLength);
        using var hostMemory = hostBuffer.AllocateMemory("Host memory", MemoryKindFlags.HostAndDeviceAccessible);
        hostBuffer.BindMemory(hostMemory);

        using (var map = hostBuffer.Map())
        {
            hostStorage.CopyTo(map[..]);
        }

        hostStorage = null;

        deviceStorage = device.CreateBuffer1D<T>();
        deviceStorage.Init("Device buffer", hostBuffer.LongCount);
        deviceMemory = deviceStorage.AllocateMemory("Device memory", MemoryKindFlags.DeviceAccessible);
        deviceStorage.BindMemory(deviceMemory);

        using var commandList = device.CreateCommandList();
        commandList.Init(new CommandList.Desc("Command list", HardwareQueueKindFlags.Compute));

        using (var cmd = commandList.Begin())
        {
            cmd.MemoryBarrier(hostBuffer, AccessFlags.HostWrite, AccessFlags.TransferRead);
            cmd.Copy(hostBuffer, deviceStorage);
            cmd.MemoryBarrier(deviceStorage, AccessFlags.TransferWrite, AccessFlags.KernelRead);
        }

        commandList.Submit();
        await Task.Run(() => commandList.CompletionFence.WaitOnCpu());
    }

    public void WriteTo(TextWriter writer)
    {
        if (hostStorage is null)
        {
            throw new NullReferenceException("The host storage was null");
        }

        writer.Write("[ ");
        for (var i = 0; i < Math.Min(hostStorage.Length, 10); i++)
        {
            writer.Write(hostStorage[i]);
            writer.Write(' ');
        }

        if (hostStorage.Length > 10)
        {
            writer.Write("... ");
        }

        writer.Write("]");
    }

    public void Dispose()
    {
        deviceMemory?.Dispose();
        deviceStorage?.Dispose();
    }
}
