namespace ZoneRV.Attributes;

/// <summary>
/// Instructs the <see cref="ZoneRV.ZoneRVJsonResolver"/> not to serialize the public field or public read/write property value.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
public class ZoneRVJsonIgnoreAttribute(JsonIgnoreType type) : Attribute
{
    public JsonIgnoreType Type { get; init; } = type;
}

public enum JsonIgnoreType
{
    Cache,
    Api,
    Both
}