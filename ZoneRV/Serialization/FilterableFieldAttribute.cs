using System.Web.Http.Controllers;

namespace ZoneRV.Serialization;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
public class FilterableFieldAttribute : Attribute;