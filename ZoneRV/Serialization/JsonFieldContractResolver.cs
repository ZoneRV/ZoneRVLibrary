using System.Reflection;
using Newtonsoft.Json.Serialization;

namespace ZoneRV.Serialization;

public class JsonFieldContractResolver : DefaultContractResolver
{
    public JsonFieldContractResolver(IEnumerable<string> fieldnames)
    {
        FieldNames = fieldnames.ToList();
    }
    
    private List<string> FieldNames = [];
    
    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        var filterAttribute = member.CustomAttributes.FirstOrDefault(x => x.AttributeType == typeof(FilterableFieldAttribute));
        
        var property = base.CreateProperty(member, memberSerialization);

        if(filterAttribute is not null && member.DeclaringType is not null)
        {
            property.ShouldSerialize =
                o => FieldNames.Any(x => x.ToLower() == $"{member.DeclaringType.Name.ToLower()}_{member.Name.ToLower()}");
        }

        return property;
    }
}