using System.Collections;
using System.Linq.Expressions;
using System.Collections.Concurrent;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using Mysqlx.Crud;

namespace ZoneRV.Models.Location;


public class LocationFactory
{
    private readonly string _serviceType;
    public LocationFactory(string serviceType)
    {
        _serviceType = serviceType;
    }
    
    public required List<ProductionWorkspace> Workspaces { get; set; }

    public IEnumerable<WorkspaceLocation> WorkspaceLocations => Workspaces.SelectMany(x => x.WorkspaceLocations);
    public IEnumerable<OrderedLineLocation> LineLocations => WorkspaceLocations.SelectMany(x => x.OrderedLineLocations);
    
    public List<string> IgnoredListNames { get; set; } = [];

    public OrderedLineLocation? GetLocationFromCustomName(ProductionLine line, string name)
    {
        if (IgnoredListNames.Contains(name))
            return null;

        return LineLocations.SingleOrDefault(x 
                                                 => x.Line == line && x.CustomNames
                                                                       .Any(y => y.CustomName.ToLower() == name.ToLower())
                                                                        );
    }
    
    public WorkspaceLocation CreateWorkspaceLocation(
        ProductionWorkspace    workspace,
        string                 locationName,
        string?                locationDescription,
        ProductionLocationType type)
    {
        if (workspace.WorkspaceLocations.Any(x => x.Name == locationName))
            throw new ArgumentException("A single workspace cannot contain 2 locations with a shared name.", nameof(locationName));
        
        WorkspaceLocation newLocation = new WorkspaceLocation()
        {
            Name = locationName,
            Description = locationDescription,
            Type = type,
            Workspace = workspace,
            OrderedLineLocations = []
        };
        
        workspace.WorkspaceLocations.Add(newLocation);

        return newLocation;
    }

    public OrderedLineLocation CreateOrderedLineLocation(ProductionLine line, WorkspaceLocation location, decimal order, string? customName, string? inventoryName)
    {
        var orderedLineLocation = line.OrderedLineLocations.SingleOrDefault(x => x.Order == order && x.Location == location);

        if (orderedLineLocation is null)
        {
            orderedLineLocation =
                new()
                {
                    Line           = line,
                    Location       = location,
                    CustomNames    = [],
                    InventoryNames = [],
                    Order          = order
                };
        
            line.OrderedLineLocations.Add(orderedLineLocation);
        }
        
        if (customName is not null)
        {
            if (orderedLineLocation.CustomNames.Any(x => x.CustomName.ToLower() == customName.ToLower()))
                throw new DuplicateNameException($"Custom name {customName} already exists for {location.Name} on {line.Name} line in the {_serviceType} service.");

            
            orderedLineLocation.CustomNames.Add(new LocationCustomName()
            {
                CustomName   = customName,
                LineLocation = orderedLineLocation,
                ServiceType  = _serviceType
            });
        }

        if (inventoryName is not null)
        {
            if (orderedLineLocation.InventoryNames.Any(x => x.CustomName.ToLower() == inventoryName.ToLower()))
                throw new DuplicateNameException($"Inventory name {inventoryName} already exists for {location.Name} on {line.Name} line in the {_serviceType} service.");
            
            orderedLineLocation.InventoryNames.Add(new LocationInventoryName()
            {
                CustomName  = inventoryName,
                LineLocation    = orderedLineLocation,
                ServiceType = _serviceType
            });
        }

        if (inventoryName is null && customName is null)
            throw new ArgumentNullException($"{nameof(customName)} and {nameof(inventoryName)} cannot both be null");
        
        return orderedLineLocation;
    }
}