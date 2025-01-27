using System.Collections;
using System.Linq.Expressions;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace ZoneRV.Models.Location;

/// <summary>
/// A factory class responsible for creating and managing instances of locations within a production environment.
/// </summary>
public class LocationFactory
{
    /// <summary>
    /// A collection of production locations used to define and manage
    /// various stages and areas in the production process.
    /// </summary>
    public required LocationCollection Locations { get; init; }

    public List<string> IgnoredListNames { get; set; } = [];

    public Location? GetLocationFromCustomName(ProductionLine? line, string name)
    {
        if (IgnoredListNames.Contains(name))
            return null;
        
        return Locations.FirstOrDefault(x => 
            (x.Line is not null && line is null) || 
            (x.Line is not null && line is not null && x.Line.Id == line.Id) && 
            x.CustomLocationNames.Any(l => l.CustomName == name));
    }

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

    /// <summary>
    /// Creates a new location with the specified details.
    /// </summary>
    /// <param name="locationName">The name of the location to create.</param>
    /// <param name="locationDescription">The description of the location.</param>
    /// <param name="locationOrder">The order or priority of the location.</param>
    /// <param name="type">The type of production location (e.g., Bay, Module, etc.).</param>
    /// <param name="productionLine">The production line associated with the location (optional).</param>
    /// <param name="bayNumber">The bay number associated with the location (optional).</param>
    /// <param name="inventoryLocations">A collection of inventory locations associated with this location (optional).</param>
    /// <returns>A newly created <see cref="Location"/> object based on the provided parameters.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when:
    /// <list type="bullet">
    /// <item><description><paramref name="productionLine"/> is null while <paramref name="type"/> is <see cref="ProductionLocationType.Bay"/>.</description></item>
    /// <item><description><paramref name="bayNumber"/> is null while <paramref name="type"/> is <see cref="ProductionLocationType.Bay"/>.</description></item>
    /// </list>
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when:
    /// <list type="bullet">
    /// <item><description>Multiple locations of type <paramref name="type"/> within the same <paramref name="productionLine"/> have the same <paramref name="locationOrder"/>.</description></item>
    /// </list>
    /// </exception>
    public Location CreateLocation(
        string                              locationName,
        string                              locationDescription,
        decimal                             locationOrder,
        ProductionLocationType              type,
        ProductionLine?                     productionLine     = null,
        int?                                bayNumber          = null,
        IEnumerable<LocationInventoryName>? inventoryLocations = null)
    {
        if (type == ProductionLocationType.Bay)
        {
            if (productionLine is null)
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

    /// <summary>
    /// Creates a preparation location with the specified details.
    /// </summary>
    /// <param name="locationName">The name of the preparation location to create.</param>
    /// <param name="locationDescription">The description of the preparation location.</param>
    /// <param name="locationOrder">The order or priority of the preparation location.</param>
    /// <param name="productionLine">The production line associated with the preparation location (optional).</param>
    /// <param name="inventoryLocations">A collection of inventory locations associated with the preparation location (optional).</param>
    /// <returns>A newly created <see cref="Location"/> object configured as a preparation location.</returns>
    /// <exception cref="ArgumentNullException">Thrown if a required argument is null.</exception>
    /// <exception cref="ArgumentException">Thrown if an argument does not meet the required criteria.</exception>
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

    /// <summary>
    /// Creates a new subassembly location with the specified details.
    /// </summary>
    /// <param name="locationName">The name of the subassembly location to create.</param>
    /// <param name="locationDescription">The description of the subassembly location.</param>
    /// <param name="locationOrder">The order or priority of the subassembly location.</param>
    /// <param name="productionLine">The production line associated with the subassembly location (optional).</param>
    /// <param name="inventoryLocations">A collection of inventory locations associated with this subassembly location (optional).</param>
    /// <returns>A newly created <see cref="Location"/> object of type Subassembly based on the provided parameters.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the location name or description is null or empty.</exception>
    /// <exception cref="ArgumentException">Thrown if the location order is invalid.</exception>
    public Location CreateSubassemblyLocation(
        string                              locationName,
        string                              locationDescription,
        decimal                             locationOrder,
        ProductionLine?                     productionLine     = null,
        IEnumerable<LocationInventoryName>? inventoryLocations = null)
    {
        return CreateLocation(locationName,   locationDescription, locationOrder, ProductionLocationType.Subassembly,
                              productionLine, null,                inventoryLocations);
    }

    /// <summary>
    /// Creates a new module location with the specified details.
    /// </summary>
    /// <param name="locationName">The name of the module location to create.</param>
    /// <param name="locationDescription">The description of the module location.</param>
    /// <param name="locationOrder">The order or priority of the module location.</param>
    /// <param name="productionLine">The production line associated with the module location (optional).</param>
    /// <param name="inventoryLocations">A collection of inventory locations associated with the module location (optional).</param>
    /// <returns>A newly created <see cref="Location"/> object based on the provided parameters, using the type <see cref="ProductionLocationType.Module"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if a required argument is null.</exception>
    /// <exception cref="ArgumentException">Thrown if an argument does not meet the required criteria.</exception>
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

    /// <summary>
    /// Creates a new bay location with the specified details.
    /// </summary>
    /// <param name="locationName">The name of the bay location to create.</param>
    /// <param name="locationDescription">The description of the bay location.</param>
    /// <param name="productionLine">The production line associated with the bay location.</param>
    /// <param name="bayNumber">The bay number associated with the location.</param>
    /// <param name="inventoryLocations">A collection of inventory locations associated with this bay location (optional).</param>
    /// <returns>A newly created <see cref="Location"/> object representing the bay location based on the provided parameters.</returns>
    /// <exception cref="ArgumentNullException">Thrown if a required argument is null.</exception>
    /// <exception cref="ArgumentException">Thrown if an argument does not meet the required criteria.</exception>
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