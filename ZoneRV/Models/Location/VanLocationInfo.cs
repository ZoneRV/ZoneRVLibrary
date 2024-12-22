using System.Collections;
using System.Diagnostics;

namespace ZoneRV.Models.Location;

[DebuggerDisplay("{CurrentLocation}")]
public class VanLocationInfo
{
    private List<(DateTimeOffset moveDate, ProductionLocation location)> _positionHistory = [];

    public ProductionLocation CurrentLocation =>
        _positionHistory.Count == 0 ? LocationFactory.PreProduction : _positionHistory.MaxBy(x => x.moveDate).location;
    
    public IReadOnlyList<(DateTimeOffset moveDate, ProductionLocation location)> PositionHistory
    {
        get => _positionHistory.AsReadOnly();
    }

    /// <exception cref="ArgumentException">Location info already contains location.</exception>
    /// <exception cref="ArgumentException">Non-bay locations Cannot be added as a location change.</exception>
    /// <exception cref="ArgumentException">Locations from different production lines cant be added to one van.</exception>
    /// <exception cref="ArgumentException">Location information already contains a location move for a date.</exception>
    private bool CheckPositionChangesValid(List<(DateTimeOffset date, ProductionLocation location)> changes)
    {
        foreach (var change in changes)
        {
            if (_positionHistory.Any(x => x.location == change.location))
                throw new ArgumentException("Location already exists", nameof(change.location));

            if (change.location.Type != ProductionLocationType.Bay && change.location != LocationFactory.PreProduction &&
                change.location != LocationFactory.PostProduction)
                throw new ArgumentException("Non bay locations Cannot be added as a location change.",
                    nameof(change.location.Type));

            if (change.location.ProductionLine is not null &&
                _positionHistory.Any(x =>
                    x.location.ProductionLine is not null && x.location.ProductionLine != change.location.ProductionLine))
                throw new ArgumentException("Location information cannot contain multiple production lines",
                    nameof(change.location.ProductionLine));

            if (_positionHistory.Any(x => x.moveDate == change.date))
                throw new ArgumentException(
                    $"Location information already contains a location move for {change.date}", nameof(change.date));
        }

        return true;
    }

    public void AddPositionChange(DateTimeOffset date, ProductionLocation location)
    {
        if (CheckPositionChangesValid([(date, location)]))
        {
            _positionHistory.Add((date, location));
            _positionHistory = _positionHistory.OrderBy(x => x.moveDate).ToList();
        }
    }
    
    public void AddPositionChangeRange(List<(DateTimeOffset date, ProductionLocation location)> changes)
    {
        if (CheckPositionChangesValid(changes))
        {
            _positionHistory.AddRange(changes);
            _positionHistory = _positionHistory.OrderBy(x => x.moveDate).ToList();
        }
    }

    public ProductionLocation GetPositionFromDate(DateTimeOffset date)
    {
        if (_positionHistory.Count == 0 || date < _positionHistory.First().moveDate)
            return LocationFactory.PreProduction;

        return _positionHistory.SkipWhile(x => date < x.moveDate).First().location;
    }

    public (DateTimeOffset start, DateTimeOffset? end)? GetDateRange(ProductionLocation position)
    {
        if (_positionHistory.All(x => x.location != position))
            return null;

        (DateTimeOffset start, DateTimeOffset? end) result;
        
        var remainingPos = _positionHistory.SkipWhile(x => x.location < position).ToList();

        result.start = remainingPos.First().moveDate;

        if (remainingPos.Count() > 1)
            result.end = remainingPos.ElementAt(1).moveDate;

        else
            result.end = null;

        return result;
    }
        
    public bool InProductionBeforeDate(DateTimeOffset end)
    {
        if (_positionHistory.All(x => x.location != LocationFactory.PostProduction))
            return false;

        return _positionHistory.First(x => x.location == LocationFactory.PostProduction).moveDate < end;
    }
}