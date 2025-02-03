using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZoneRV.Models.Location;

public class OrderedLineLocation : IEquatable<OrderedLineLocation>
{
    public required ProductionLine    Line              { get; set; }
    public required LineLocation      LineLocation      { get; set; }
    public          WorkspaceLocation WorkspaceLocation => LineLocation.WorkspaceLocation;
    
    public required decimal Order { get; set; }

    [JsonIgnore] public LineLocationType Type 
        => Order == decimal.MinValue ? LineLocationType.PreProduction : 
           Order == decimal.MaxValue ? LineLocationType.PostProduction : 
           LineLocationType.Production;
    
    public static bool operator <(OrderedLineLocation first, OrderedLineLocation second)
    {
        return first.Order < second.Order;
    }
    
    public static bool operator >(OrderedLineLocation first, OrderedLineLocation second)
    {
        return first.Order > second.Order;
    }

    public static bool operator ==(OrderedLineLocation first, OrderedLineLocation second)
    {
        return first.GetHashCode() == second.GetHashCode();
    }

    public static bool operator !=(OrderedLineLocation first, OrderedLineLocation second)
    {
        return !(first == second);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Order, Line);
    }

    public int GetHashCode(OrderedLineLocation obj)
        => obj.GetHashCode();

    public bool Equals(OrderedLineLocation? other)
    {
        if (other is null)
            return false;
        
        if (ReferenceEquals(this, other))
            return true;
        
        return GetHashCode() == other.GetHashCode() && 
               Line.Equals(other.Line);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null)
            return false;
        if (ReferenceEquals(this, obj))
            return true;
        if (obj.GetType() != GetType())
            return false;
        return Equals((OrderedLineLocation)obj);
    }
}