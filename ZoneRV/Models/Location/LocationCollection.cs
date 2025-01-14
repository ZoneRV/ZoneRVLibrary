﻿using System.Collections;

namespace ZoneRV.Models.Location;

[DebuggerDisplay("{_locations.Count} locations")]
public class LocationCollection : IEnumerable<ProductionLocation>
{
    private readonly List<ProductionLocation> _locations = [];
    
    public IEnumerator<ProductionLocation> GetEnumerator()
        => _locations.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
    
    public LocationCollection(IEnumerable<ProductionLocation> initialLocations)
    {
        _locations.Add(LocationFactory.PreProduction);
        _locations.Add(LocationFactory.PostProduction);
        
        _locations.AddRange(initialLocations);
    }

    internal void Add(ProductionLocation newLocation)
    {
        _locations.Add(newLocation);
    }

    public ProductionLocation? GetBay(ProductionLine line, int bayNumber)
    {
        return _locations.SingleOrDefault(x => x.ProductionLine is not null && x.ProductionLine == line && x.BayNumber == bayNumber);
    }
}