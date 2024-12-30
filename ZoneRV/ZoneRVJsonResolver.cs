using System.Reflection;
using Newtonsoft.Json.Serialization;

namespace ZoneRV;

public class ZoneRVJsonResolver(JsonIgnoreType ignoreType) : DefaultContractResolver
{
    private JsonIgnoreType IgnoreType { get; set; } = ignoreType;
    
    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        var property = base.CreateProperty(member, memberSerialization);

        var attribute = member.GetCustomAttribute<ZoneRVJsonIgnoreAttribute>();
        
        if (attribute is not null && attribute.Type == IgnoreType)
        {
            property.ShouldSerialize = instance => false;
        }

        return property;
    }
}