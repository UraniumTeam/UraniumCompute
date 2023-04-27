namespace UraniumCompute.Generator;

internal class TranslatedMethod
{
    public string Name { get; }
    public string Code { get; }

    internal TranslatedMethod(string name, string code)
    {
        Name = name;
        Code = code;
    }
}
