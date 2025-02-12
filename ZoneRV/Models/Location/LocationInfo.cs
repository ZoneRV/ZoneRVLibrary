using System.Collections;
using ZoneRV.Serialization;

namespace ZoneRV.Models.Location;

/// <summary>
/// Represents location-specific information, including historical movements and current position.
/// Provides utilities to track and retrieve location change data over time.
/// </summary>
[DebuggerDisplay("{CurrentLocation}:{_line.Name} - {_locationHistory.Count} locations")]
public class LocationInfo : IEnumerable<(DateTimeOffset moveDate, OrderedLineLocation lineLocation)>
{
    [OptionalJsonField] private List<(DateTimeOffset moveDate, OrderedLineLocation lineLocation)> _locationHistory;
    
    private ProductionLine _line { get; init; }

    public LocationInfo(ProductionLine line, IEnumerable<(DateTimeOffset moveDate, OrderedLineLocation lineLocation)> history)
    {
        _line = line;
        _locationHistory = history.ToList();
    }
    
    public LocationInfo(ProductionLine line)
    {
        _line = line;
        _locationHistory = [];
    }

    /// <summary>
    /// Gets the current location of the entity if available.
    /// Returns the most recent location based on the movement history,
    /// or null if no location data exists.
    /// </summary>
    public OrderedLineLocation? CurrentLocation =>
        _locationHistory.Count == 0 ? null : _locationHistory.MaxBy(x => x.moveDate).lineLocation;

    /// <summary>
    /// Validates the provided position changes to ensure they can be added to the location history.
    /// </summary>
    /// <param name="changes">The list of position changes, each containing a date and an OrderedLineLocation, to validate.</param>
    /// <returns>True if all position changes are valid.</returns>
    /// <exception cref="ArgumentException">Thrown when a location in the changes already exists in the location history.</exception>
    /// <exception cref="ArgumentException">Thrown when a non-bay location is included in the changes.</exception>
    /// <exception cref="ArgumentException">Thrown when the changes include locations from multiple production lines.</exception>
    /// <exception cref="ArgumentException">Thrown when a position change date in the changes already exists in the location history.</exception>
    private bool CheckPositionChangesValid(List<(DateTimeOffset date, OrderedLineLocation lineLocation)> changes)
    {
        foreach (var change in changes)
        {
            if (_locationHistory.Any(x => x.lineLocation == change.lineLocation))
                throw new ArgumentException("Location already exists", nameof(change.lineLocation));

            if (change.lineLocation.Location.LocationType is not ProductionLocationType.Bay )
                throw new ArgumentException("Non bay locations Cannot be added as a location change.",
                    nameof(change.lineLocation.Location.LocationType));

            if (change.lineLocation.Line is not null && _locationHistory
                    .Any(x => x.lineLocation.Line != change.lineLocation.Line))
                throw new ArgumentException("Location information cannot contain multiple production lines",
                    nameof(change.lineLocation.Line));

            if (_locationHistory.Any(x => x.moveDate == change.date))
                throw new ArgumentException(
                    $"Location information already contains a location move for {change.date}", nameof(change.date));
        }

        return true;
    }

    /// <summary>
    /// Adds a new position change to the location history if the change is valid.
    /// </summary>
    /// <param name="date">The date of the position change.</param>
    /// <param name="location">The new OrderedLineLocation associated with the position change.</param>
    /// <exception cref="ArgumentException">Thrown if the position change is invalid based on validation rules.</exception>
    public void AddPositionChange(DateTimeOffset date, OrderedLineLocation location)
    {
        if (CheckPositionChangesValid([(date, lineLocation: location)]))
        {
            _locationHistory.Add((date, location));
            _locationHistory = _locationHistory.OrderBy(x => x.moveDate).ToList();
        }
    }

    /// <summary>
    /// Adds a range of position changes to the location history and orders the history by move date after insertion.
    /// </summary>
    /// <param name="changes">The list of position changes, each containing a move date and an OrderedLineLocation, to be added to the location history.</param>
    /// <exception cref="ArgumentException">Thrown when the position changes are invalid, such as containing duplicate move dates, non-bay locations, or locations from different production lines.</exception>
    public void AddPositionChangeRange(List<(DateTimeOffset date, OrderedLineLocation lineLocation)> changes)
    {
        if (CheckPositionChangesValid(changes))
        {
            _locationHistory.AddRange(changes);
            _locationHistory = _locationHistory.OrderBy(x => x.moveDate).ToList();
        }
    }

    /// <summary>
    /// Retrieves the position associated with a specific date from the location history.
    /// </summary>
    /// <param name="date">The date to find the corresponding position for.</param>
    /// <returns>The OrderedLineLocation if a position is found for the provided date; otherwise, null if no position exists or the date is earlier than the first recorded move date.</returns>
    public OrderedLineLocation? GetPositionFromDate(DateTimeOffset date)
    {
        if (_locationHistory.Count == 0 || date < _locationHistory.First().moveDate)
            return null;

        return _locationHistory.SkipWhile(x => date < x.moveDate).First().lineLocation;
    }

    /// <summary>
    /// Retrieves the date range during which the specified location was active.
    /// </summary>
    /// <param name="position">The location to find the date range for.</param>
    /// <returns>A tuple containing the start date and the end date (or null if no end date exists) when the location was active, or null if the location is not found in the history.</returns>
    public (DateTimeOffset start, DateTimeOffset? end)? GetDateRange(OrderedLineLocation position)
    {
        if (_locationHistory.All(x => x.lineLocation != position))
            return null;

        (DateTimeOffset start, DateTimeOffset? end) result;
        
        var remainingPos = _locationHistory.SkipWhile(x => x.lineLocation < position).ToList();

        result.start = remainingPos.First().moveDate;

        if (remainingPos.Count() > 1)
            result.end = remainingPos.ElementAt(1).moveDate;

        else
            result.end = null;

        return result;
    }

    public IEnumerator<(DateTimeOffset moveDate, OrderedLineLocation lineLocation)> GetEnumerator()
    {
        return _locationHistory.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}