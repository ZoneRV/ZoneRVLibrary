using System.Collections;

namespace ZoneRV.Models.Location;

/// <summary>
/// Represents a collection of locations, providing functionality to access and manage locations
/// within the collection.
/// </summary>
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

    /// <summary>
    /// Adds a new location to the collection. Ensures no duplicate locations are added
    /// by comparing the hash codes of the locations.
    /// </summary>
    /// <param name="newLocation">The location to be added to the collection.</param>
    /// <exception cref="Exception">Thrown when the specified location already exists in the collection.</exception>
    internal void Add(Location newLocation)
    {
        if (_locations.Any(x => x.GetHashCode() == newLocation.GetHashCode()))
            throw new Exception("Location already Exists in collection");
        
        _locations.Add(newLocation);
    }

    /// <summary>
    /// Retrieves a specific location within the collection based on the production line
    /// and bay number provided.
    /// </summary>
    /// <param name="line">The production line associated with the desired location.</param>
    /// <param name="bayNumber">The bay number identifying the specific location within the production line.</param>
    /// <returns>The location that matches the provided production line and bay number, or null if no match is found.</returns>
    public Location? GetBay(ProductionLine line, int bayNumber)
    {
        return _locations.SingleOrDefault(x => x.Line is not null && x.Line == line && x.BayNumber == bayNumber);
    }
    
    
    public IEnumerable<Location> GetAllLocationsFromLine(ProductionLine? line)
        => _locations.Where(x => x.Line == line);
}