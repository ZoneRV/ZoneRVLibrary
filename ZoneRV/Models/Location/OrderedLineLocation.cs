﻿using System.ComponentModel.DataAnnotations;
using ZoneRV.Serialization;

namespace ZoneRV.Models.Location;

/// <summary>
/// Represents a location within a production line, with a specified order.
/// </summary>
[DebuggerDisplay("{Location.Name}:{Line.Name} order: {Order}")]
public class OrderedLineLocation : IEquatable<OrderedLineLocation>
{
    [Key, Required] public int Id { get; init; }

    [OptionalJsonField] public required ProductionLine    Line     { get; set; }
    [OptionalJsonField] public required WorkspaceLocation Location { get; set; }
    
    [Required]
    public required decimal Order { get; set; }

    
    [JsonIgnore] public required virtual ICollection<LocationCustomName>    CustomNames    { get; set; }
    [JsonIgnore] public required virtual ICollection<LocationInventoryName> InventoryNames { get; set; }
    
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