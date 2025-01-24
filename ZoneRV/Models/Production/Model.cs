using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZoneRV.Models.Production;

[DebuggerDisplay("{Name} - {ProductionLine.Name}")]
[Table("Model")]
public class Model
{
    [Key, Required] public required int Id { get; set; }
    
    [ForeignKey("Line")] public required int LineId { get; set; }
    
    [Required, MaxLength(128)] public required string Name { get; set; }
    
    [MaxLength(1024)] public required string? Description { get; set; }
    
    [Required, MaxLength(10)] public required string Prefix { get; set; }
    
    
    
    public required ProductionLine ProductionLine { get; set; }
}