using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ZoneRV.Serialization;

namespace ZoneRV.Models.Production;

/// <summary>
/// Represents a production line, which is a collection of production models in a manufacturing context.
/// </summary>
[DebuggerDisplay("{Name}")]
[Table("Line")]
public class ProductionLine
{
    [Key, Required] public int Id { get; set; }
    
    [Required, MaxLength(128)]
    public required string Name { get; set; }
    
    [MaxLength(1024)]
    public string? Description { get; set; }

    [OptionalJsonField, Required, ForeignKey("WorkspaceId"), DeleteBehavior(DeleteBehavior.NoAction)] // TODO: Figure out the deletion behaviour
    public required virtual ProductionWorkspace Workspace { get; init; }
    
    public virtual required ICollection<OrderedLineLocation> OrderedLineLocations { get; set; }
    
    [OptionalJsonField] public List<Model>         Models    { get; set; } = [];

    [OptionalJsonField] public List<AreaOfOrigin> AreaOfOrigins { get; set; } = [];

    public static bool operator ==(ProductionLine? first, ProductionLine? second)
        => (first is null && second is null) || 
           first is not null && 
           second is not null && 
           first.Id == second.Id;

    public static bool operator !=(ProductionLine? first, ProductionLine? second)
        => (first is not null && second is null) ||
           (first is null && second is not null) || 
           first is not null && 
           second is not null && 
           first.Id != second.Id;
}