using System.Reflection;

namespace ZoneRV.Serialization;

public static class SerializationUtils
{
    private static Dictionary<string, (bool productinoLoadRequired, bool inventoryLoadRequired)> _fieldRequirements = [];
    private static List<Type> _checkedTypes = [];  
    
    static SerializationUtils()
    {
        
        var types = typeof(AreaOfOrigin).Assembly.GetTypes();
        
        foreach (var type in types
                    .Where(x =>
                                x.IsClass && 
                                !x.IsAbstract &&
                                !x.Name.Contains("<>c") &&
                                x.Namespace is not null && 
                                x.Namespace.StartsWith("ZoneRV.Models")))
        {
            AddTypePropertiesToDictionary(type);
        }
    }

    public static void CheckForRequired(string field, out bool productionRequired, out bool inventoryRequired)
    {
        if (_fieldRequirements.TryGetValue(field.ToLower(), out var value))
        {
            productionRequired = value.productinoLoadRequired;
            inventoryRequired  = value.inventoryLoadRequired;

            return;
        }

        productionRequired = false;
        inventoryRequired  = false;
    }
    
    public static string PropertyInfoToFieldName(PropertyInfo propertyInfo)
        => $"{propertyInfo.ReflectedType!.Name.ToLower()}_{propertyInfo.Name.ToLower()}";

    public static string MemberInfoToFieldName(MemberInfo memberInfo)
        => $"{memberInfo.ReflectedType!.Name.ToLower()}_{memberInfo.Name.ToLower()}";

    private static void AddTypePropertiesToDictionary(Type type)
    {
        if (_checkedTypes.Contains(type.ReflectedType ?? type))
            return;
        
        foreach (var propertyInfo in type.GetProperties())
        {
            AddPropertyToDictionary(propertyInfo);
        }
    }
    
    private static void AddPropertyToDictionary(PropertyInfo propertyInfo)
    {
        var att = propertyInfo.GetCustomAttribute<OptionalJsonFieldAttribute>();

        if (att is not null)
        {
            _fieldRequirements.Add(PropertyInfoToFieldName(propertyInfo), (att.RequiresProductionLoaded, att.RequiresInventoryLoaded) );
        }
    }
}