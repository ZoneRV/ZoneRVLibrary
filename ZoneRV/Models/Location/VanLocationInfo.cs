using System.Collections;
using System.Diagnostics;

namespace ZoneRV.Models.Location;

[DebuggerDisplay("{CurrentLocation}")]
public class VanLocationInfo
{
    private List<(DateTimeOffset moveDate, ProductionLocation location)> _locationHistory = [];

    public ProductionLocation CurrentLocation =>
        _locationHistory.Count == 0 ? LocationFactory.PreProduction : _locationHistory.MaxBy(x => x.moveDate).location;
    
    public IReadOnlyList<(DateTimeOffset moveDate, ProductionLocation location)> LocationHistory
    {
        get => _locationHistory.AsReadOnly();
        init => _locationHistory = value.ToList();
    }

    /// <exception cref="ArgumentException">Location info already contains location.</exception>
    /// <exception cref="ArgumentException">Non-bay locations Cannot be added as a location change.</exception>
    /// <exception cref="ArgumentException">Locations from different production lines cant be added to one van.</exception>
    /// <exception cref="ArgumentException">Location information already contains a location move for a date.</exception>
    private bool CheckPositionChangesValid(List<(DateTimeOffset date, ProductionLocation location)> changes)
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

    public void AddPositionChange(DateTimeOffset date, ProductionLocation location)
    {
        if (CheckPositionChangesValid([(date, location)]))
        {
            _locationHistory.Add((date, location));
            _locationHistory = _locationHistory.OrderBy(x => x.moveDate).ToList();
        }
    }
    
    public void AddPositionChangeRange(List<(DateTimeOffset date, ProductionLocation location)> changes)
    {
        if (CheckPositionChangesValid(changes))
        {
            _locationHistory.AddRange(changes);
            _locationHistory = _locationHistory.OrderBy(x => x.moveDate).ToList();
        }
    }

    public ProductionLocation GetPositionFromDate(DateTimeOffset date)
    {
        if (_locationHistory.Count == 0 || date < _locationHistory.First().moveDate)
            return LocationFactory.PreProduction;

        return _locationHistory.SkipWhile(x => date < x.moveDate).First().location;
    }

    public (DateTimeOffset start, DateTimeOffset? end)? GetDateRange(ProductionLocation position)
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
        
    public bool InProductionBeforeDate(DateTimeOffset end)
    {
        if (_locationHistory.All(x => x.location != LocationFactory.PostProduction))
            return false;

        return _locationHistory.First(x => x.location == LocationFactory.PostProduction).moveDate < end;
    }
}