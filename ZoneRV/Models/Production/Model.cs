using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ZoneRV.Serialization;

namespace ZoneRV.Models.Production;

/// <summary>
/// Represents a production model with attributes such as name, description, prefix,
/// and its associated production line.
/// </summary>
[DebuggerDisplay("{Name} - {Line.Name}")]
[Table("Model")]
public class Model
{
    [Key, Required] public                     int Id { get; set; }
    
    [Required, MaxLength(128)] public required string Name { get; set; }
    
    [MaxLength(1024)] public required string? Description { get; set; }
    
    //TODO: add a model ype (eg van}
    
    [Required, MaxLength(10)] public required string Prefix { get; set; }
    
    
    
    [ForeignKey("LineId"), OptionalJsonField] public required ProductionLine Line { get; set; }
}