using System.Collections;
using System.Diagnostics;
using ZoneRV.Serialization;

namespace ZoneRV.Models.Location;

/// <summary>
/// Represents location-specific information, including historical movements and current position.
/// Provides utilities to track and retrieve location change data over time.
/// </summary>
[DebuggerDisplay("{CurrentLocation}")]
public class LocationInfo : IEnumerable<(DateTimeOffset moveDate, Location location)>
{
    [OptionalJsonField] private List<(DateTimeOffset moveDate, Location location)> _locationHistory;

    public LocationInfo(IEnumerable<(DateTimeOffset moveDate, Location location)> history)
    {
        _locationHistory = history.ToList();
    }
    
    public LocationInfo()
    {
        _locationHistory = [];
    }

    /// <summary>
    /// Gets the current location of the entity, determined by the most recent move date
    /// in the location history. If no location history exists, it defaults to the pre-production location.
    /// </summary>
    /// <value>
    /// The latest <see cref="Location"/> based on the move date in the history or
    /// a default pre-production location if the history is empty.
    /// </value>
    public Location CurrentLocation =>
        _locationHistory.Count == 0 ? LocationFactory.PreProduction : _locationHistory.MaxBy(x => x.moveDate).location;

    /// <exception cref="ArgumentException">Location info already contains location.</exception>
    /// <exception cref="ArgumentException">Non-bay locations Cannot be added as a location change.</exception>
    /// <exception cref="ArgumentException">Locations from different production lines cant be added to one van.</exception>
    /// <exception cref="ArgumentException">Location information already contains a location move for a date.</exception>
    private bool CheckPositionChangesValid(List<(DateTimeOffset date, Location location)> changes)
    {
        foreach (var change in changes)
        {
            if (_locationHistory.Any(x => x.location == change.location))
                throw new ArgumentException("Location already exists", nameof(change.location));

            if (change.location.Type != ProductionLocationType.Bay && change.location != LocationFactory.PreProduction &&
                change.location != LocationFactory.PostProduction)
                throw new ArgumentException("Non bay locations Cannot be added as a location change.",
                    nameof(change.location.Type));

            if (change.location.Line is not null &&
                _locationHistory.Where(x => x.location.Line is not null)
                    .Any(x => x.location.Line != change.location.Line))
                throw new ArgumentException("Location information cannot contain multiple production lines",
                    nameof(change.location.Line));

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
    public void AddPositionChange(DateTimeOffset date, Location location)
    {
        if (CheckPositionChangesValid([(date, location)]))
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
    public void AddPositionChangeRange(List<(DateTimeOffset date, Location location)> changes)
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
    public Location GetPositionFromDate(DateTimeOffset date)
    {
        if (_locationHistory.Count == 0 || date < _locationHistory.First().moveDate)
            return LocationFactory.PreProduction;

        return _locationHistory.SkipWhile(x => date < x.moveDate).First().location;
    }

    /// <summary>
    /// Retrieves the date range during which a specified location was active.
    /// </summary>
    /// <param name="position">The location to retrieve the active date range for.</param>
    /// <returns>A tuple containing the start date and, if available, the end date during which the location was active.
    /// Returns <c>null</c> if the location is not present in the location history.</returns>
    public (DateTimeOffset start, DateTimeOffset? end)? GetDateRange(Location position)
    {
        if (_locationHistory.All(x => x.location != position))
            return null;

        (DateTimeOffset start, DateTimeOffset? end) result;
        
        var remainingPos = _locationHistory.SkipWhile(x => x.location < position).ToList();

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
        if (_locationHistory.All(x => x.location != LocationFactory.PostProduction))
            return true;

        return _locationHistory.First(x => x.location == LocationFactory.PostProduction).moveDate > end;
    }

    public IEnumerator<(DateTimeOffset moveDate, Location location)> GetEnumerator()
    {
        return _locationHistory.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}