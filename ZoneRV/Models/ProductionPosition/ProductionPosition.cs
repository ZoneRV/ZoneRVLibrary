using System.Diagnostics;

namespace ZoneRV.Models.ProductionPosition;

[DebuggerDisplay("{ProductionLine} - {PositionName}")]
public class ProductionPosition : IEquatable<ProductionPosition>, IEqualityComparer<ProductionPosition>, IComparable<ProductionPosition>
{
    public ProductionLine? ProductionLine { get; set; }
    public required string PositionName { get; set; }
    public required string PositionDescription { get; set; }
    public required List<string> InventoryLocations { get; set; } = [];
    
    public required decimal PositionOrder { get; set; }
    public required ProductionPositionType Type { get; init; }

    public static readonly ProductionPosition PreProduction = new ProductionPosition()
    {
        PositionName = "Pre Production",
        PositionDescription = "Production has not yet started",
        InventoryLocations = [],
        PositionOrder = Decimal.MinValue,
        Type = ProductionPositionType.Prep
    };

    public static readonly ProductionPosition PostProduction = new ProductionPosition()
    {
        PositionName = "Post Production",
        PositionDescription = "Production has finished",
        InventoryLocations = [],
        PositionOrder = Decimal.MaxValue,
        Type = ProductionPositionType.Finishing
    };

    public static bool operator <(ProductionPosition first, ProductionPosition second)
    {
        return first.PositionOrder < second.PositionOrder;
    }

    public static bool operator >(ProductionPosition first, ProductionPosition second)
    {
        return first.PositionOrder > second.PositionOrder;
    }

    public static bool operator ==(ProductionPosition first, ProductionPosition second)
    {
        return first.ProductionLine == second.ProductionLine && 
               first.Type == second.Type && 
               first.PositionOrder == second.PositionOrder;
    }

    public static bool operator !=(ProductionPosition first, ProductionPosition second)
    {
        return first.ProductionLine != second.ProductionLine || 
               first.Type != second.Type || 
               first.PositionOrder != second.PositionOrder;
    }
    
    public bool Equals(ProductionPosition? other)
    {
        if (other is null) 
            return false;
        
        if (ReferenceEquals(this, other)) 
            return true;
        
        return ProductionLine == other.ProductionLine && 
               PositionName == other.PositionName && 
               PositionDescription == other.PositionDescription && 
               InventoryLocations.Equals(other.InventoryLocations) && 
               PositionOrder == other.PositionOrder && 
               Type == other.Type;
    }

    public int CompareTo(ProductionPosition? other)
    {
        ArgumentNullException.ThrowIfNull(other);

        if (PositionOrder == other.PositionOrder)
            return 0;
        
        if(PositionOrder < other.PositionOrder)
            return -1;

        return 1;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) 
            return false;
        
        if (ReferenceEquals(this, obj)) 
            return true;
        
        if (obj.GetType() != GetType()) 
            return false;
        
        return Equals((ProductionPosition)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine((ProductionLine is null ? -1 : (int)ProductionLine), PositionName, PositionDescription, InventoryLocations, PositionOrder, (int)Type);
    }

    public int GetHashCode(ProductionPosition obj)
    {
        return obj.GetHashCode();
    }

    public bool Equals(ProductionPosition? x, ProductionPosition? y)
    {
        if(x is null)
            return false;
        
        return x.Equals(y);
    }
}