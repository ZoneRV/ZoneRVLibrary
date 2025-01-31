using System.Web.Http.Controllers;

namespace ZoneRV.Serialization;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
public class OptionalJsonFieldAttribute(bool requiresProductionLoaded = false, bool requiresInventoryLoaded = false) : Attribute
{
    public bool RequiresProductionLoaded { get; set; } = requiresProductionLoaded;
    public bool RequiresInventoryLoaded { get; set; } = requiresInventoryLoaded;
}