using System.Diagnostics;
using System.Reflection;
using ZoneRV.Models.Location;

namespace ZoneRV.Tests;

public class VanModels
{
    [Fact]
    public void AllModelsHaveDebuggerDisplay()
    {
        Type[] ignoredTypes = [typeof(LocationFactory)];  
        var types = typeof(ZoneRV.Models.Van.AreaOfOrigin).Assembly.GetTypes();
        
        foreach (var type in types
                     .Where(x => !ignoredTypes.Contains(x) && 
                                      x.IsClass && 
                                      !x.IsAbstract &&
                                      !x.Name.Contains("<>c") &&
                                      x.Namespace is not null && 
                                      x.Namespace.StartsWith("ZoneRV.Models")))
        {
            Assert.True(type.GetCustomAttribute<DebuggerDisplayAttribute>() is not null, $"'{type.Name}' in {type.Namespace} does not contain a {nameof(DebuggerDisplayAttribute)}");
        }
    }
}