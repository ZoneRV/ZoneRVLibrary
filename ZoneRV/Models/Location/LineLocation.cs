using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZoneRV.Models.Location;


[Table("LineLocation")]
public class LineLocation : IEquatable<LineLocation>
{
    [Key, Required] public int Id { get; set; }
    
    [Required] public required ProductionLine    Line              { get; set; }
    [Required] public required WorkspaceLocation WorkspaceLocation { get; set; }
    
    [JsonIgnore] public virtual ICollection<LocationInventoryName>  InventoryLocations  { get; set; } = default!;
    [JsonIgnore] public virtual ICollection<LineLocationCustomName> CustomLocationNames { get; set; } = default!;
    
    /// <summary>
    ///Use <see cref="LocationFactory"/>  to create locations
    /// </summary>
    internal LineLocation()
    {
        
    }

    public static bool operator ==(LineLocation first, LineLocation second)
    {
        return first.GetHashCode() == second.GetHashCode();
    }

    public static bool operator !=(LineLocation first, LineLocation second)
    {
        return !(first == second);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public int GetHashCode(LineLocation obj)
        => obj.GetHashCode();

    public bool Equals(LineLocation? other)
    {
        if (other is null)
            return false;
        
        if (ReferenceEquals(this, other))
            return true;
        
        return Id == other.Id && 
               Line.Equals(other.Line) && 
               WorkspaceLocation.Equals(other.WorkspaceLocation) && 
               InventoryLocations.Equals(other.InventoryLocations) && 
               CustomLocationNames.Equals(other.CustomLocationNames);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null)
            return false;
        if (ReferenceEquals(this, obj))
            return true;
        if (obj.GetType() != GetType())
            return false;
        return Equals((LineLocation)obj);
    }
}