using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ZoneRV.Serialization;

namespace ZoneRV.Models.Location;

[DebuggerDisplay("{Line} - {Name}")]
[Table("WorkspaceLocation")]
public class WorkspaceLocation : IEquatable<WorkspaceLocation>
{
    [Key, Required] public int Id { get; init; }
    
    [OptionalJsonField, Required] public virtual required ProductionWorkspace Workspace { get; init; }
    public virtual required ICollection<OrderedLineLocation> OrderedLineLocations { get; set; }
    
    [MaxLength(128)] public required string Name { get; set; }
    
    [MaxLength(1024)] public string? Description { get; set; }


    public required ProductionLocationType LocationType { get; init; }

    /// <summary>
    ///Use <see cref="LocationFactory"/>  to create locations
    /// </summary>
    internal WorkspaceLocation()
    {
        
    }

    public static bool operator ==(WorkspaceLocation first, WorkspaceLocation second)
    {
        return first.GetHashCode() == second.GetHashCode();
    }

    public static bool operator !=(WorkspaceLocation first, WorkspaceLocation second)
    {
        return first.GetHashCode() != second.GetHashCode();
    }
    
    public bool Equals(WorkspaceLocation? other)
    {
        if (other is null) 
            return false;
        
        if (ReferenceEquals(this, other)) 
            return true;
        
        return Name == other.Name && 
               Workspace.Id == other.Workspace.Id &&
               Description == other.Description && 
               LocationType == other.LocationType;
    }
    public override bool Equals(object? obj)
    {
        if (obj is null) 
            return false;
        
        if (ReferenceEquals(this, obj)) 
            return true;
        
        if (obj.GetType() != GetType()) 
            return false;
        
        return Equals((WorkspaceLocation)obj);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public int GetHashCode(WorkspaceLocation obj)
        => obj.GetHashCode();

    public bool Equals(WorkspaceLocation? x, WorkspaceLocation? y)
    {
        if(x is null)
            return false;
        
        return x.Equals(y);
    }
}