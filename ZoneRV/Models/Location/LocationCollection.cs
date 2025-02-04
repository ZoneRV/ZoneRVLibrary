using System.Collections;

namespace ZoneRV.Models.Location;


[DebuggerDisplay("{_locations.Count} locations")]
public class OrderedLineLocationCollection : IEnumerable<OrderedLineLocation>
{
    private readonly List<OrderedLineLocation> _locations;
    private readonly List<ProductionLine>      _lines;
    
    public IEnumerator<OrderedLineLocation> GetEnumerator()
        => _locations.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
    
    public OrderedLineLocationCollection(IEnumerable<ProductionLine> lines, IEnumerable<OrderedLineLocation> initialLocations)
    {
        _lines     = lines.ToList();
        _locations = [];
        
        _locations.AddRange(initialLocations);
    }
    
    public OrderedLineLocationCollection(IEnumerable<ProductionLine> lines)
    {
        _lines     = lines.ToList();
        _locations = [];
    }

    /// <summary>
    /// Adds a new location to the collection. Ensures no duplicate locations are added
    /// by comparing the hash codes of the locations.
    /// </summary>
    /// <param name="newLocation">The location to be added to the collection.</param>
    /// <exception cref="Exception">Thrown when the specified location already exists in the collection.</exception>
    internal void Add(OrderedLineLocation newLocation)
    {
        if (_locations.Any(x => x.GetHashCode() == newLocation.GetHashCode()))
            throw new Exception("Location already Exists in collection");
        
        _locations.Add(newLocation);
    }
}