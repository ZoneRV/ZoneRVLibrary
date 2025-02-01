using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ZoneRV.Serialization;

namespace ZoneRV.Models.Production;

/// <summary>
/// Represents a specific production model within a production line.
/// </summary>
/// <remarks>
/// This class is used to define properties specific to a production model such as its name,
/// description, prefix, and the associated production line.
/// The class is mapped to a database table named "Model" and contains validation attributes
/// for entity constraints.
/// </remarks>
[DebuggerDisplay("{Name} - {Line.Name}")]
[Table("Model")]
public class Model
{
    [Key, Required] public                     int Id { get; set; }
    
    [ForeignKey("Line"), JsonIgnore] public required int LineId { get; set; }
    
    [Required, MaxLength(128)] public required string Name { get; set; }
    
    [MaxLength(1024)] public required string? Description { get; set; }
    
    //TODO: add a model ype (eg van}
    
    [Required, MaxLength(10)] public required string Prefix { get; set; }
    
    
    
    [ForeignKey("LineId"), OptionalJsonField] public required ProductionLine Line { get; set; }
}