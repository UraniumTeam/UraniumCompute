namespace UraniumCompute.Acceleration;

public readonly struct AdapterInfo
{
    public readonly int Id;
    public readonly string Name;
    public readonly AdapterKind Kind;

    public AdapterInfo(int id, string name, AdapterKind kind)
    {
        Id = id;
        Name = name;
        Kind = kind;
    }
}
