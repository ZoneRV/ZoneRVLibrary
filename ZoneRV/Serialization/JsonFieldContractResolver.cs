using System.Reflection;
using Newtonsoft.Json.Serialization;

namespace ZoneRV.Serialization;

public class JsonFieldContractResolver : DefaultContractResolver
{
    public JsonFieldContractResolver(IEnumerable<string> fieldNames)
    {
        _fieldNames = fieldNames.ToList();
    }
    
    private readonly List<string> _fieldNames;
    
    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        var filterAttribute = member.CustomAttributes.FirstOrDefault(x => x.AttributeType == typeof(OptionalJsonFieldAttribute));
        
        var property = base.CreateProperty(member, memberSerialization);

        if(filterAttribute is not null && member.DeclaringType is not null)
        {
            property.ShouldSerialize =
                o => _fieldNames.Any(x => 
                    x.Equals(
                        $"{member.DeclaringType.Name.ToLower()}_{member.Name.ToLower()}", 
                        StringComparison.CurrentCultureIgnoreCase));
        }

        return property;
    }
}