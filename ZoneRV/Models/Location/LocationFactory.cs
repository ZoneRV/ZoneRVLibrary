using System.Collections;
using System.Linq.Expressions;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace ZoneRV.Models.Location;


public class LocationFactory
{
    public required LocationCollection Locations { get; init; }

    public IReadOnlyList<string> IgnoredListNames
    {
        get => _ignoredListNames.AsReadOnly();
        init => _ignoredListNames = value.ToList();
    }
        
    private List<string> _ignoredListNames = [];

    public Location? GetLocationFromCustomName(ProductionLine? line, string name)
    {
        if (_ignoredListNames.Contains(name))
            return null;
        
        return Locations.FirstOrDefault(x => 
            (x.Line is not null && line is null) || 
            (x.Line is not null && line is not null && x.Line.Id == line.Id) && 
            x.CustomLocationNames.Any(l => l.CustomName == name));
    }

    public IEnumerable<Location> GetAllLocationsFromLine(ProductionLine? line)
        => Locations.Where(x => x.Line == line);
    
    /// <summary>
    /// Default location for new vans
    /// </summary>
    public static readonly Location PreProduction = new Location()
    {
        Name = "Pre Production",
        Description = "Production has not yet started",
        Order = decimal.MinValue,
        Type = ProductionLocationType.Prep
    };

    /// <summary>
    /// Default location for vans after completion
    /// </summary>
    public static readonly Location PostProduction = new Location()
    {
        Name = "Post Production",
        Description = "Production has finished",
        Order = decimal.MaxValue,
        Type = ProductionLocationType.Finishing
    };

    public Location CreateLocation(
        string locationName, 
        string locationDescription, 
        decimal locationOrder, 
        ProductionLocationType type, 
        ProductionLine? productionLine = null, 
        int? bayNumber = null, 
        IEnumerable<LocationInventoryName>? inventoryLocations = null)
    {
        if (type == ProductionLocationType.Bay)
        {
            if(productionLine is null)
            {
                throw new ArgumentNullException(nameof(productionLine),
                    "Production line cannot be null if location is a bay");
            }
            
            if (Locations.Any(x =>
                    x.Type == type && 
                    x.Line is not null &&
                    x.Line == productionLine && 
                    x.Order == locationOrder))
                throw new ArgumentException("Multiple bays cannot have the same location order", nameof(locationOrder));
            
            if(bayNumber is null)
                throw new ArgumentNullException(nameof(bayNumber), "Bay number cannot be null if location is a bay");
        }
        
        Location newLocation = new Location()
        {
            Name = locationName,
            Description = locationDescription,
            Order = locationOrder,
            Type = type,
            Line = productionLine,
            BayNumber = bayNumber,
            InventoryLocations = inventoryLocations is null ? [] : inventoryLocations.ToList()
        };
        
        Locations.Add(newLocation);

        return newLocation;
    }

    public Location CreatePrepLocation(
        string                              locationName, 
        string                              locationDescription, 
        decimal                             locationOrder, 
        ProductionLine?                     productionLine     = null, 
        IEnumerable<LocationInventoryName>? inventoryLocations = null)
    {
        return CreateLocation(locationName, locationDescription, locationOrder, ProductionLocationType.Prep,
            productionLine, null, inventoryLocations);
    }

    public Location CreateSubassemblyLocation(
        string                              locationName, 
        string                              locationDescription, 
        decimal                             locationOrder, 
        ProductionLine?                     productionLine     = null, 
        IEnumerable<LocationInventoryName>? inventoryLocations = null)
    {
        return CreateLocation(locationName, locationDescription, locationOrder, ProductionLocationType.Subassembly,
            productionLine, null, inventoryLocations);
    }

    public Location CreateModuleLocation(
        string                              locationName, 
        string                              locationDescription, 
        decimal                             locationOrder, 
        ProductionLine?                     productionLine     = null, 
        IEnumerable<LocationInventoryName>? inventoryLocations = null)
    {
        return CreateLocation(locationName, locationDescription, locationOrder, ProductionLocationType.Module,
            productionLine, null, inventoryLocations);
    }

    public Location CreateBayLocation(
        string                              locationName, 
        string                              locationDescription, 
        ProductionLine                      productionLine,
        int                                 bayNumber, 
        IEnumerable<LocationInventoryName>? inventoryLocations = null)
    {
        return CreateLocation(locationName, locationDescription, bayNumber, ProductionLocationType.Bay,
            productionLine, bayNumber, inventoryLocations);
    }
}