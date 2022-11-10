using UraniumCompute.Containers;

namespace ContainersTests;

[TestFixture]
public class NativeArrayTests
{
    [Test]
    public void Create()
    {
        var array = new NativeArray<int>(32);
        for (var i = 0; i < array.Count; ++i)
        {
            array[i] = i;
        }

        CollectionAssert.AreEqual(Enumerable.Range(0, 32), array);
        array.Dispose();
    }

    [Test]
    public void Resize()
    {
        var array = new NativeArray<int>();
        array.Resize(32);
        for (var i = 0; i < array.Count; ++i)
        {
            array[i] = i;
        }

        CollectionAssert.AreEqual(Enumerable.Range(0, 32), array);
        array.Dispose();
    }

    [Test]
    public void FromArray()
    {
        var array = new[] { 1, 2, 3 };
        var nativeArray = new NativeArray<int>(array);
        CollectionAssert.AreEqual(array, nativeArray);
        nativeArray.Dispose();
    }
}
