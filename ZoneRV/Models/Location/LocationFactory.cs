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

    public ProductionLocation? GetLocationFromCustomName(ProductionLine? line, string name)
    {
        if (_ignoredListNames.Contains(name))
            return null;
        
        return Locations.First(x => 
            (x.ProductionLine is not null && line is null) || 
            (x.ProductionLine is not null && line is not null && x.ProductionLine.Id == line.Id) && 
            x.CustomNames.Contains(name));
    }

    public IEnumerable<ProductionLocation> GetAllLocationsFromLine(ProductionLine? line)
        => Locations.Where(x => x.ProductionLine == line);
    
    /// <summary>
    /// Default location for new vans
    /// </summary>
    public static readonly ProductionLocation PreProduction = new ProductionLocation()
    {
        Name = "Pre Production",
        Description = "Production has not yet started",
        Order = decimal.MinValue,
        Type = ProductionLocationType.Prep
    };

    /// <summary>
    /// Default location for vans after completion
    /// </summary>
    public static readonly ProductionLocation PostProduction = new ProductionLocation()
    {
        Name = "Post Production",
        Description = "Production has finished",
        Order = decimal.MaxValue,
        Type = ProductionLocationType.Finishing
    };

    public ProductionLocation CreateLocation(
        string locationName, 
        string locationDescription, 
        decimal locationOrder, 
        ProductionLocationType type, 
        ProductionLine? productionLine = null, 
        int? bayNumber = null, 
        IEnumerable<string>? inventoryLocations = null)
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
                    x.ProductionLine is not null &&
                    x.ProductionLine == productionLine && 
                    x.Order == locationOrder))
                throw new ArgumentException("Multiple bays cannot have the same location order", nameof(locationOrder));
            
            if(bayNumber is null)
                throw new ArgumentNullException(nameof(bayNumber), "Bay number cannot be null if location is a bay");
        }
        
        ProductionLocation newLocation = new ProductionLocation()
        {
            Name = locationName,
            Description = locationDescription,
            Order = locationOrder,
            Type = type,
            ProductionLine = productionLine,
            BayNumber = bayNumber,
            InventoryLocations = inventoryLocations is null ? [] : inventoryLocations.ToList()
        };
        
        Locations.Add(newLocation);

        return newLocation;
    }

    public ProductionLocation CreatePrepLocation(
        string locationName, 
        string locationDescription, 
        decimal locationOrder, 
        ProductionLine? productionLine = null, 
        IEnumerable<string>? inventoryLocations = null)
    {
        return CreateLocation(locationName, locationDescription, locationOrder, ProductionLocationType.Prep,
            productionLine, null, inventoryLocations);
    }

    public ProductionLocation CreateSubassemblyLocation(
        string locationName, 
        string locationDescription, 
        decimal locationOrder, 
        ProductionLine? productionLine = null, 
        IEnumerable<string>? inventoryLocations = null)
    {
        return CreateLocation(locationName, locationDescription, locationOrder, ProductionLocationType.Subassembly,
            productionLine, null, inventoryLocations);
    }

    public ProductionLocation CreateModuleLocation(
        string locationName, 
        string locationDescription, 
        decimal locationOrder, 
        ProductionLine? productionLine = null, 
        IEnumerable<string>? inventoryLocations = null)
    {
        return CreateLocation(locationName, locationDescription, locationOrder, ProductionLocationType.Module,
            productionLine, null, inventoryLocations);
    }

    public ProductionLocation CreateBayLocation(
        string locationName, 
        string locationDescription, 
        ProductionLine productionLine,
        int bayNumber, 
        IEnumerable<string>? inventoryLocations = null)
    {
        return CreateLocation(locationName, locationDescription, bayNumber, ProductionLocationType.Bay,
            productionLine, bayNumber, inventoryLocations);
    }
}