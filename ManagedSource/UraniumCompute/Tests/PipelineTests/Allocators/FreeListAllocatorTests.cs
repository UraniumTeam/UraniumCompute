using UraniumCompute.Acceleration.Allocators;

namespace PipelineTests.Allocators;

public class FreeListAllocatorTests
{
    [Test]
    public void FreeListAllocator_EmptyWhenConstructed()
    {
        var allocator = new FreeListDeviceAllocator();
        allocator.Init(new IDeviceAllocator.Desc(128));
        Assert.That(allocator.AllocatedByteCount, Is.EqualTo(0));

        allocator = new FreeListDeviceAllocator(FreeListDeviceAllocator.Policy.FirstFit);
        allocator.Init(new IDeviceAllocator.Desc(128));
        Assert.That(allocator.AllocatedByteCount, Is.EqualTo(0));
    }

    [Test]
    public void FreeListAllocator_AllocatesMemory()
    {
        var allocator = new FreeListDeviceAllocator();
        allocator.Init(new IDeviceAllocator.Desc(512));
        var handle = allocator.Allocate(512);
        Assert.That(allocator.AllocatedByteCount, Is.GreaterThanOrEqualTo(512));
        Assert.Multiple(() =>
        {
            Assert.That(handle.IsValid, Is.True);
            Assert.That((ulong)handle, Is.EqualTo(0));
        });
    }

    [Test]
    public void FreeListAllocator_AllocatesMemoryTwice()
    {
        var allocator = new FreeListDeviceAllocator();
        allocator.Init(new IDeviceAllocator.Desc(512));
        var a = allocator.Allocate(256);
        Assert.That(allocator.AllocatedByteCount, Is.GreaterThanOrEqualTo(256));
        var b = allocator.Allocate(256);
        Assert.That(allocator.AllocatedByteCount, Is.GreaterThanOrEqualTo(512));
        Assert.Multiple(() =>
        {
            Assert.That(a.IsValid, Is.True);
            Assert.That((ulong)a, Is.EqualTo(0));
            Assert.That(b.IsValid, Is.True);
            Assert.That((ulong)b, Is.EqualTo(256));
        });
    }

    [Test]
    public void FreeListAllocator_AllocatesLessThanAlignment()
    {
        var allocator = new FreeListDeviceAllocator();
        allocator.Init(new IDeviceAllocator.Desc(512));
        var a = allocator.Allocate(1);
        Assert.That(allocator.AllocatedByteCount, Is.GreaterThanOrEqualTo(1));
        var b = allocator.Allocate(256);
        Assert.That(allocator.AllocatedByteCount, Is.GreaterThanOrEqualTo(512));
        Assert.Multiple(() =>
        {
            Assert.That(a.IsValid, Is.True);
            Assert.That((ulong)a, Is.EqualTo(0));
            Assert.That(b.IsValid, Is.True);
            Assert.That((ulong)b, Is.EqualTo(256));
        });
    }

    [Test]
    public void FreeListAllocator_AllocatesZeroBytes()
    {
        var allocator = new FreeListDeviceAllocator();
        allocator.Init(new IDeviceAllocator.Desc(256));
        var handle = allocator.Allocate(0);
        Assert.That(allocator.AllocatedByteCount, Is.EqualTo(0));
        Assert.Multiple(() =>
        {
            Assert.That(handle.IsValid, Is.False);
            Assert.That(handle.IsNull, Is.True);
        });
    }

    [Test]
    public void FreeListAllocator_DeAllocatesMemory()
    {
        var allocator = new FreeListDeviceAllocator();
        allocator.Init(new IDeviceAllocator.Desc(512));
        var a = allocator.Allocate(256);
        Assert.That(allocator.AllocatedByteCount, Is.EqualTo(256));
        allocator.DeAllocate(a);
        allocator.GarbageCollectForce();
        Assert.That(allocator.AllocatedByteCount, Is.EqualTo(0));
        var b = allocator.Allocate(256);
        Assert.Multiple(() =>
        {
            Assert.That(allocator.AllocatedByteCount, Is.EqualTo(256));
            Assert.That(b, Is.EqualTo(a));
        });
    }

    [Test]
    public void FreeListAllocator_AllocatesMemoryRandom()
    {
        const int runCount = 10000;

        var allocator = new FreeListDeviceAllocator();
        allocator.Init(new IDeviceAllocator.Desc(10000 * 1024));
        var random = new Random(123);
        var handles = new List<NullableHandle>(runCount);
        var allocatedTotal = 0L;
        for (var i = 0; i < runCount; ++i)
        {
            var bytes = random.NextInt64(0, 1024);
            var handle = allocator.Allocate((ulong)bytes);
            handles.Add(handle);
            allocatedTotal += bytes;
        }

        Assert.That(allocator.AllocatedByteCount, Is.GreaterThanOrEqualTo(allocatedTotal));

        var allocatedByteCount = allocator.AllocatedByteCount;
        allocator.DeAllocate(handles.Last());
        allocator.GarbageCollectForce();
        Assert.That(allocator.AllocatedByteCount, Is.LessThan(allocatedByteCount));

        handles.RemoveAt(handles.Count - 1);

        Shuffle(random, handles);
        foreach (var handle in handles)
        {
            allocator.DeAllocate(handle);
        }

        allocator.GarbageCollectForce();
        Assert.That(allocator.AllocatedByteCount, Is.EqualTo(0));
    }

    private static void Shuffle<T>(Random random, IList<T> source)
    {
        var n = source.Count;
        while (n > 1)
        {
            var k = random.Next(n--);
            (source[n], source[k]) = (source[k], source[n]);
        }
    }
}
