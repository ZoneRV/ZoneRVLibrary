using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZoneRV.Models.Location;

/// <summary>
/// Represents a location in the production process.
/// </summary>
/// <remarks>
/// The Location class supports comparison, equality, and ordering functions, including custom operators.
/// </remarks>
[DebuggerDisplay("{Line} - {Name}")]
[Table("Location")]
public class Location : IEquatable<Location>, IEqualityComparer<Location>, IComparable<Location>
{
    [Key, Required] public int Id { get; init; }
    
    public virtual ProductionLine? Line { get; init; }
    
    [MaxLength(128)] public required string Name { get; set; }
    
    [MaxLength(1024)] public required string Description { get; set; }

    [JsonIgnore] public virtual ICollection<LocationInventoryName> InventoryLocations  { get; set; } = default!;
    [JsonIgnore] public virtual ICollection<LocationCustomName>    CustomLocationNames { get; set; } = default!;

    public decimal Order
    {
        get => _order;

        internal set
        {
            if (Type == ProductionLocationType.Bay)
                _order = (int)value;

            else
                _order = value;
        }
    }

    
    private decimal _order;
    
    public required ProductionLocationType Type { get; init; }
    
    public int? BayNumber { get; init; }

    /// <summary>
    ///Use <see cref="LocationFactory"/>  to create locations
    /// </summary>
    internal Location()
    {
        
    }

    public static bool operator <(Location first, Location second)
    {
        return first.Order < second.Order;
    }

    public static bool operator >(Location first, Location second)
    {
        return first.Order > second.Order;
    }

    public static bool operator ==(Location first, Location second)
    {
        return first.GetHashCode() == second.GetHashCode();
    }

    public static bool operator !=(Location first, Location second)
    {
        return first.GetHashCode() != second.GetHashCode();
    }
    
    public bool Equals(Location? other)
    {
        if (other is null) 
            return false;
        
        if (ReferenceEquals(this, other)) 
            return true;
        
        return (Line is null && other.Line is null) ||
               (Line is not null && other.Line is not null) &&
               Line == other.Line && 
               Name == other.Name && 
               Description == other.Description && 
               InventoryLocations.Equals(other.InventoryLocations) && 
               Order == other.Order && 
               Type == other.Type;
    }

    public int CompareTo(Location? other)
    {
        ArgumentNullException.ThrowIfNull(other);
        
        if(Order < other.Order)
            return -1;
        
        if(Order > other.Order)
            return 1;

        if(Type < other.Type)
            return -1;

        if(Type > other.Type)
            return 1;

        return 0;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) 
            return false;
        
        if (ReferenceEquals(this, obj)) 
            return true;
        
        if (obj.GetType() != GetType()) 
            return false;
        
        return Equals((Location)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine((Line is null ? -1 : Line.Id), Name, Order, (int)Type);
    }

    public int GetHashCode(Location obj)
        => obj.GetHashCode();

    public bool Equals(Location? x, Location? y)
    {
        if(x is null)
            return false;
        
        return x.Equals(y);
    }
}