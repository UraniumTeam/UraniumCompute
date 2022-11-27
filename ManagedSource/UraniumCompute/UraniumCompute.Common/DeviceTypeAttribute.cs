namespace UraniumCompute.Common;

[AttributeUsage(AttributeTargets.Struct)]
public sealed class DeviceTypeAttribute : Attribute
{
    public string DeviceTypeName { get; }

    public DeviceTypeAttribute(string deviceTypeName)
    {
        DeviceTypeName = deviceTypeName;
    }
}
