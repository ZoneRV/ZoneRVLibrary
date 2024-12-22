using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace ZoneRV.Models.Location;

[DebuggerDisplay("{ProductionLine} - {LocationName}")]
public class ProductionLocation : IEquatable<ProductionLocation>, IEqualityComparer<ProductionLocation>, IComparable<ProductionLocation>
{
    public ProductionLine? ProductionLine { get; init; }
    
    public required string LocationName { get; set; }
    
    public required string LocationDescription { get; set; }
    
    public List<string> InventoryLocations { get; set; } = [];

    public decimal LocationOrder
    {
        get => _locationOrder;

        internal set
        {
            if (Type == ProductionLocationType.Bay)
                _locationOrder = (int)value;

            else
                _locationOrder = value;
        }
    }

    [JsonIgnore]
    private decimal _locationOrder;
    
    public required ProductionLocationType Type { get; init; }
    
    public int? BayNumber { get; init; }

    /// <summary>
    ///Use <see cref="LocationFactory"/>  to create locations
    /// </summary>
    internal ProductionLocation()
    {
        
    }

    public static bool operator <(ProductionLocation first, ProductionLocation second)
    {
        return first.LocationOrder < second.LocationOrder;
    }

    public static bool operator >(ProductionLocation first, ProductionLocation second)
    {
        return first.LocationOrder > second.LocationOrder;
    }

    public static bool operator ==(ProductionLocation first, ProductionLocation second)
    {
        return first.GetHashCode() == second.GetHashCode();
    }

    public static bool operator !=(ProductionLocation first, ProductionLocation second)
    {
        return first.GetHashCode() != second.GetHashCode();
    }
    
    public bool Equals(ProductionLocation? other)
    {
        if (other is null) 
            return false;
        
        if (ReferenceEquals(this, other)) 
            return true;
        
        return ProductionLine == other.ProductionLine && 
               LocationName == other.LocationName && 
               LocationDescription == other.LocationDescription && 
               InventoryLocations.Equals(other.InventoryLocations) && 
               LocationOrder == other.LocationOrder && 
               Type == other.Type;
    }

    public int CompareTo(ProductionLocation? other)
    {
        ArgumentNullException.ThrowIfNull(other);
        
        if(LocationOrder < other.LocationOrder)
            return -1;
        
        if(LocationOrder > other.LocationOrder)
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
        
        return Equals((ProductionLocation)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine((ProductionLine is null ? -1 : (int)ProductionLine), LocationName, LocationOrder, (int)Type);
    }

    public int GetHashCode(ProductionLocation obj)
        => obj.GetHashCode();

    public bool Equals(ProductionLocation? x, ProductionLocation? y)
    {
        if(x is null)
            return false;
        
        return x.Equals(y);
    }
}