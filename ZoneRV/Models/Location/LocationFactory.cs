﻿using System.Collections;
using System.Linq.Expressions;
using System.Collections.Concurrent;

namespace ZoneRV.Models.Location;


public class LocationFactory
{
    public readonly LocationCollection Locations = new LocationCollection();
    
    /// <summary>
    /// Default location for new vans
    /// </summary>
    public static readonly ProductionLocation PreProduction = new ProductionLocation()
    {
        LocationName = "Pre Production",
        LocationDescription = "Production has not yet started",
        LocationOrder = decimal.MinValue,
        Type = ProductionLocationType.Prep
    };

    /// <summary>
    /// Default location for vans after completion
    /// </summary>
    public static readonly ProductionLocation PostProduction = new ProductionLocation()
    {
        LocationName = "Post Production",
        LocationDescription = "Production has finished",
        LocationOrder = decimal.MaxValue,
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
                    x.ProductionLine == productionLine && 
                    x.LocationOrder == locationOrder))
                throw new ArgumentException("Multiple bays cannot have the same location order", nameof(locationOrder));
            
            if(bayNumber is null)
                throw new ArgumentNullException(nameof(bayNumber), "Bay number cannot be null if location is a bay");
        }
        
        ProductionLocation newLocation = new ProductionLocation()
        {
            LocationName = locationName,
            LocationDescription = locationDescription,
            LocationOrder = locationOrder,
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
        ProductionLine? productionLine, 
        IEnumerable<string>? inventoryLocations = null)
    {
        return CreateLocation(locationName, locationDescription, locationOrder, ProductionLocationType.Module,
            productionLine, null, inventoryLocations);
    }

    public ProductionLocation CreateGen2BayLocation(
        string locationName, 
        string locationDescription, 
        int bayNumber, 
        IEnumerable<string>? inventoryLocations = null)
    {
        return CreateLocation(locationName, locationDescription, bayNumber, ProductionLocationType.Bay,
            ProductionLine.Gen2, bayNumber, inventoryLocations);
    }

    public ProductionLocation CreateExpoBayLocation(
        string locationName, 
        string locationDescription, 
        int bayNumber, 
        IEnumerable<string>? inventoryLocations = null)
    {
        return CreateLocation(locationName, locationDescription, bayNumber, ProductionLocationType.Bay,
            ProductionLine.Expo, bayNumber, inventoryLocations);
    }
}