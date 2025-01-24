using System.Collections;

namespace ZoneRV.Models.Location;

[DebuggerDisplay("{_locations.Count} locations")]
public class LocationCollection : IEnumerable<Location>
{
    private readonly List<Location> _locations = [];
    
    public IEnumerator<Location> GetEnumerator()
        => _locations.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
    
    public LocationCollection(IEnumerable<Location> initialLocations)
    {
        _locations.Add(LocationFactory.PreProduction);
        _locations.Add(LocationFactory.PostProduction);
        
        _locations.AddRange(initialLocations);
    }
    
    public LocationCollection()
    {
        _locations.Add(LocationFactory.PreProduction);
        _locations.Add(LocationFactory.PostProduction);
    }

    internal void Add(Location newLocation)
    {
        _locations.Add(newLocation);
    }

    public Location? GetBay(ProductionLine line, int bayNumber)
    {
        return _locations.SingleOrDefault(x => x.Line is not null && x.Line == line && x.BayNumber == bayNumber);
    }
}