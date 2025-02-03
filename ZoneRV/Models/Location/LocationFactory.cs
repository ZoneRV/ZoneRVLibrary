using System.Collections;
using System.Linq.Expressions;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace ZoneRV.Models.Location;


public class LocationFactory
{
    public required List<ProductionWorkspace> Workspaces { get; set; }

    public IEnumerable<WorkspaceLocation> WorkspaceLocations => Workspaces.SelectMany(x => x.WorkspaceLocations);
    public IEnumerable<LineLocation>      LineLocations      => WorkspaceLocations.SelectMany(x => x.LineLocations);
    
    public List<string> IgnoredListNames { get; set; } = [];

    public OrderedLineLocation? GetLocationFromCustomName(ProductionLine line, string name)
    {
        if (IgnoredListNames.Contains(name))
            return null;
        
        var lineLocation = LineLocations.FirstOrDefault(x => 
            x.Line.Id == line.Id && 
            x.CustomLocationNames.Any(l => l.CustomName == name));

        if (lineLocation is null)
            return null;
        
        return new OrderedLineLocation()
        {
            Line = line,
            Location = lineLocation,
            Order = lineLocation.CustomLocationNames.First(l => l.CustomName == name).Order
        };
    }

    /// <summary>
    /// Default location for new vans
    /// </summary>
    public static OrderedLineLocation PreProduction(ProductionLine line)
    => new()
    {
        Order = decimal.MinValue,
        Line  = line,
        Location = new LineLocation()
        {
            WorkspaceLocation = new WorkspaceLocation()
            {
            Name        = "Pre Production",
            Description = "Production has not yet started",
            Type        = ProductionLocationType.Prep,
            Workspace   = line.Workspace
        }, 
        Line                = line, 
        CustomLocationNames = [], 
        InventoryLocations  = []
        }
    };

    /// <summary>
    /// Default location for vans after completion
    /// </summary>
    public static OrderedLineLocation PostProduction(ProductionLine line) 
        => new()
    {
        Order = decimal.MaxValue,
        Line = line,
        Location = new LineLocation()
        {
            WorkspaceLocation = new WorkspaceLocation()
            {
                Name        = "Post Production",
                Description = "Production has finished",
                Type        = ProductionLocationType.Finishing,
                Workspace   = line.Workspace
            }, 
            Line = line, 
            CustomLocationNames = [], 
            InventoryLocations = []
        }
    };
    
    public WorkspaceLocation CreateWorkspaceLocation(
        ProductionWorkspace    workspace,
        string                 locationName,
        string                 locationDescription,
        ProductionLocationType type)
    {
        if (workspace.WorkspaceLocations.Any(x => x.Name == locationName))
            throw new ArgumentException("A single workspace cannot contain 2 locations with a shared name.", nameof(locationName));
        
        WorkspaceLocation newLocation = new WorkspaceLocation()
        {
            Name = locationName,
            Description = locationDescription,
            Type = type,
            Workspace = workspace
        };
        
        workspace.WorkspaceLocations.Add(newLocation);

        return newLocation;
    }
}