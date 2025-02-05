using System.Reflection;
using Newtonsoft.Json.Serialization;

namespace ZoneRV.Serialization;

public class JsonFieldContractResolver : DefaultContractResolver
{
    private readonly List<string> _fieldNames;
    
    public JsonFieldContractResolver(IEnumerable<string> fieldNames)
    {
        _fieldNames    = fieldNames.ToList();
        NamingStrategy = new CamelCaseNamingStrategy();
    }

    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        var filterAttribute = member.CustomAttributes.FirstOrDefault(x => x.AttributeType == typeof(OptionalJsonFieldAttribute));
        
        var property = base.CreateProperty(member, memberSerialization);

        if(filterAttribute is not null && member.ReflectedType is not null)
        {
            property.ShouldSerialize =
                o => _fieldNames.Any(x => 
                    x.Equals(
                        SerializationUtils.MemberInfoToFieldName(member), 
                        StringComparison.CurrentCultureIgnoreCase));
        }

        return property;
    }
}