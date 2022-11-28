namespace CompilerTests;

public struct TestStruct
{
    public float X;
    public float Y;
}

public partial class DecompilerTests
{
    [Test]
    public void CompilesUserStruct()
    {
        var expectedResult = @"";

        AssertFunc(() =>
        {
            var s = new TestStruct { X = 0.5f, Y = 1.5f };
            return s.X + s.Y;
        }, expectedResult);
    }
}
