using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using ZoneRV.Serialization;

namespace ZoneRV.Api;

public static class ZoneJsonSerializerSettings
{
    private static Dictionary<string, (bool requireProductionLoad, bool requireInventoryLoad)> _fieldRequirements = [];
    private static List<Type> _checkedTypes = [];
    
    static ZoneJsonSerializerSettings()
    {
        var types = typeof(SalesOrder).Assembly.GetTypes();

        foreach (var type in types
                     .Where(x => x.IsClass &&
                                      !x.IsAbstract &&
                                      !x.Name.Contains("<>c") &&
                                      x.Namespace is not null &&
                                      x.Namespace.StartsWith("ZoneRV.Models")))
        {
            foreach (var member in type.GetProperties())
            {
                AddMemberToDictionary(member);
            }
        }
        
        _checkedTypes.Clear();
    }

    private static void AddMemberToDictionary(PropertyInfo prop)
    {
        var propType = prop.PropertyType;
        
        if(_checkedTypes.Contains(propType))
            return;
        
        _checkedTypes.Add(propType);
        
        var attribute = prop.GetCustomAttribute<OptionalJsonFieldAttribute>();

        if (attribute is not null && prop.DeclaringType is not null)
        {
            var memberName = $"{prop.DeclaringType.Name.ToLower()}_{prop.Name.ToLower()}";
            
            if(!_fieldRequirements.ContainsKey(memberName))
                _fieldRequirements.Add(memberName, (attribute.RequiresProductionLoaded, attribute.RequiresInventoryLoaded));
            
        }
        
        if (propType.Namespace is not null && propType.Namespace.StartsWith("ZoneRV"))
        {
            foreach (var subMember in propType.GetProperties())
            {
                AddMemberToDictionary(subMember);
            }
        }
    }

    public static JsonSerializerSettings GetOptionalSerializerSettings(IEnumerable<string> includedFields)
    {
        var settings = new JsonSerializerSettings()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            ContractResolver = new JsonFieldContractResolver(includedFields)
        };
        
        settings.Converters.Add(new StringEnumConverter(new CamelCaseNamingStrategy()));

        return settings;
    }
}