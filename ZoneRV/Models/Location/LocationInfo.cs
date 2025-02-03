using System.Collections;
using System.Diagnostics;
using ZoneRV.Serialization;

namespace ZoneRV.Models.Location;

/// <summary>
/// Represents location-specific information, including historical movements and current position.
/// Provides utilities to track and retrieve location change data over time.
/// </summary>
[DebuggerDisplay("{CurrentLocation}")]
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
    /// Gets the current location of the entity, determined by the most recent move date
    /// in the location history. If no location history exists, it defaults to the pre-production location.
    /// </summary>
    /// <value>
    /// The latest <see cref="WorkspaceLocation"/> based on the move date in the history or
    /// a default pre-production location if the history is empty.
    /// </value>
    public OrderedLineLocation CurrentLocation =>
        _locationHistory.Count == 0 ? LocationFactory.PreProduction(_line) : _locationHistory.MaxBy(x => x.moveDate).lineLocation;

    /// <exception cref="ArgumentException">Location info already contains location.</exception>
    /// <exception cref="ArgumentException">Non-bay locations Cannot be added as a location change.</exception>
    /// <exception cref="ArgumentException">Locations from different production lines cant be added to one van.</exception>
    /// <exception cref="ArgumentException">Location information already contains a location move for a date.</exception>
    private bool CheckPositionChangesValid(List<(DateTimeOffset date, OrderedLineLocation lineLocation)> changes)
    {
        foreach (var change in changes)
        {
            if (_locationHistory.Any(x => x.lineLocation == change.lineLocation))
                throw new ArgumentException("Location already exists", nameof(change.lineLocation));

            if (change.lineLocation.LineLocation.WorkspaceLocation.Type != ProductionLocationType.Bay && change.lineLocation.Type == LineLocationType.Production)
                throw new ArgumentException("Non bay locations Cannot be added as a location change.",
                    nameof(change.lineLocation.Type));

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
    /// Adds a position change to the location history.
    /// </summary>
    /// <param name="date">The date of the position change.</param>
    /// <param name="location">The location to be added.</param>
    /// <exception cref="ArgumentException">Thrown when the location information already contains the specified location.</exception>
    /// <exception cref="ArgumentException">Thrown when a non-bay location is added as a position change, unless it is pre or post-production.</exception>
    /// <exception cref="ArgumentException">Thrown when trying to add locations from different production lines.</exception>
    /// <exception cref="ArgumentException">Thrown when a position change already exists for the specified date.</exception>
    public void AddPositionChange(DateTimeOffset date, OrderedLineLocation location)
    {
        if (CheckPositionChangesValid([(date, lineLocation: location)]))
        {
            _locationHistory.Add((date, location));
            _locationHistory = _locationHistory.OrderBy(x => x.moveDate).ToList();
        }
    }

    /// <summary>
    /// Adds a range of position changes to the location history. Ensures the changes are valid before adding them
    /// and reorders the history based on the movement dates.
    /// </summary>
    /// <param name="changes">The list of position changes, each containing a date and a location, to be added to the history.</param>
    /// <exception cref="ArgumentException">Location info already contains location.</exception>
    /// <exception cref="ArgumentException">Non-bay locations cannot be added as a location change.</exception>
    /// <exception cref="ArgumentException">Locations from different production lines cannot be added to one van.</exception>
    /// <exception cref="ArgumentException">Location information already contains a location move for a date.</exception>
    public void AddPositionChangeRange(List<(DateTimeOffset date, OrderedLineLocation lineLocation)> changes)
    {
        if (CheckPositionChangesValid(changes))
        {
            _locationHistory.AddRange(changes);
            _locationHistory = _locationHistory.OrderBy(x => x.moveDate).ToList();
        }
    }

    /// <summary>
    /// Retrieves the location corresponding to the specified date from the location history.
    /// </summary>
    /// <param name="date">The date for which the location is to be retrieved.</param>
    /// <returns>The location associated with the specified date. If the date is earlier than the first movement in the history, returns the pre-production location.</returns>
    public OrderedLineLocation GetPositionFromDate(DateTimeOffset date)
    {
        if (_locationHistory.Count == 0 || date < _locationHistory.First().moveDate)
            return LocationFactory.PreProduction(_line);

        return _locationHistory.SkipWhile(x => date < x.moveDate).First().lineLocation;
    }

    /// <summary>
    /// Retrieves the date range during which a specified location was active.
    /// </summary>
    /// <param name="position">The location to retrieve the active date range for.</param>
    /// <returns>A tuple containing the start date and, if available, the end date during which the location was active.
    /// Returns <c>null</c> if the location is not present in the location history.</returns>
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

    /// <summary>
    /// Determines whether the van exited the production line after the specified date.
    /// </summary>
    /// <param name="end">The date to check against.</param>
    /// <returns>
    /// Returns true if the van has not yet reached the "Post Production" location or exited it after the specified date;
    /// otherwise, false.
    /// </returns>
    public bool ExitedAfterDate(DateTimeOffset end)
    {
        if (_locationHistory.All(x => x.lineLocation.Type != LineLocationType.PostProduction))
            return true;

        return _locationHistory.First(x => x.lineLocation.Type == LineLocationType.PostProduction).moveDate > end;
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