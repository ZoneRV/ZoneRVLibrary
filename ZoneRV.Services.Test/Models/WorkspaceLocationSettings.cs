using ZoneRV.Models.Enums;

namespace ZoneRV.Services.Test.Models;

public class WorkspaceLocationSettings
{
    public WorkspaceLocationSettings(string? name = null, string? description = null, ProductionLocationType? type = null)
    {
        Name         = name;
        Description  = description;
        LocationType = type;
    }
    
    public string? Name        { get; set;}
    public string? Description { get; set;}
    public ProductionLocationType? LocationType { get; set;}
}