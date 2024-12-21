using System.Diagnostics;

namespace ZoneRV.Models.ProductionPosition;

[DebuggerDisplay("{CurrentPosition}")]
public class PositionInfo
{
    private List<(DateTimeOffset moveDate, ProductionPosition position)> _positionHistory = [];

    public ProductionPosition CurrentPosition =>
        _positionHistory.Count == 0 ? ProductionPosition.PreProduction : _positionHistory.MaxBy(x => x.moveDate).position;

    public void AddPositionChange(DateTimeOffset date, ProductionPosition position)
    {
        if (_positionHistory.Any(x => x.position == position))
            throw new ArgumentException(
                $"Position information already contains a position move for {position.PositionName}", nameof(position));
        
        if (_positionHistory.Any(x => x.moveDate == date))
            throw new ArgumentException(
                $"Position information already contains a position move for {date}", nameof(date));
        
        _positionHistory.Add((date, position));

        _positionHistory = _positionHistory.OrderBy(x => x.moveDate).ToList();
    }
    
    public void AddPositionChangeRange(List<(DateTimeOffset date, ProductionPosition position)> changes)
    {
        foreach (var change in changes)
        {
            if (_positionHistory.Any(x => x.position == change.position))
                throw new ArgumentException(
                    $"Position information already contains a position move for {change.position.PositionName}", nameof(change.position));
        
            if (_positionHistory.Any(x => x.moveDate == change.date))
                throw new ArgumentException(
                    $"Position information already contains a position move for {change.date}", nameof(change.date));
        }
        
        _positionHistory.AddRange(changes);

        _positionHistory = _positionHistory.OrderBy(x => x.moveDate).ToList();
    }

    public ProductionPosition GetPositionFromDate(DateTimeOffset date)
    {
        if (_positionHistory.Count == 0 || date < _positionHistory.First().moveDate)
            return ProductionPosition.PreProduction;

        return _positionHistory.SkipWhile(x => date < x.moveDate).First().position;
    }

    public (DateTimeOffset start, DateTimeOffset? end)? GetDateRange(ProductionPosition position)
    {
        if (_positionHistory.All(x => x.position != position))
            return null;

        (DateTimeOffset start, DateTimeOffset? end) result;
        
        var remainingPos = _positionHistory.SkipWhile(x => x.position < position).ToList();

        result.start = remainingPos.First().moveDate;

        if (remainingPos.Count() > 1)
            result.end = remainingPos.ElementAt(1).moveDate;

        else
            result.end = null;

        return result;
    }
        
    public bool InProductionBeforeDate(DateTimeOffset end)
    {
        if (_positionHistory.All(x => x.position != ProductionPosition.PostProduction))
            return false;

        return _positionHistory.First(x => x.position == ProductionPosition.PostProduction).moveDate < end;
    }
}