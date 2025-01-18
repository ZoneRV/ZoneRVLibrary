namespace ZoneRV.Models.Location;

[DebuggerDisplay("{ProductionLine} - {Name}")]
public class ProductionLocation : IEquatable<ProductionLocation>, IEqualityComparer<ProductionLocation>, IComparable<ProductionLocation>
{
    public int Id { get; init; }
    
    public ProductionLine? ProductionLine { get; init; }
    
    public required string Name { get; set; }
    
    public required string Description { get; set; }
    
    public string? BayLeaderId { get; set; }
    
    public List<string> InventoryLocations { get; set; } = [];
    public List<string> CustomNames { get; set; } = [];

    [ZoneRVJsonIgnore(JsonIgnoreType.Both)] public decimal Order
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
    internal ProductionLocation()
    {
        
    }

    public static bool operator <(ProductionLocation first, ProductionLocation second)
    {
        return first.Order < second.Order;
    }

    public static bool operator >(ProductionLocation first, ProductionLocation second)
    {
        return first.Order > second.Order;
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
        
        return (ProductionLine is null && other.ProductionLine is null) ||
               (ProductionLine is not null && other.ProductionLine is not null) &&
               ProductionLine == other.ProductionLine && 
               Name == other.Name && 
               Description == other.Description && 
               InventoryLocations.Equals(other.InventoryLocations) && 
               Order == other.Order && 
               Type == other.Type;
    }

    public int CompareTo(ProductionLocation? other)
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
        
        return Equals((ProductionLocation)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine((ProductionLine is null ? -1 : ProductionLine.Id), Name, Order, (int)Type);
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