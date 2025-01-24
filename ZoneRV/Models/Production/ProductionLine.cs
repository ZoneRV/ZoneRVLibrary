using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZoneRV.Models.Production;

[DebuggerDisplay("{Name}")]
[Table("Line")]
public class ProductionLine
{
    [Key, Required] public required int    Id   { get; set; }
    [MaxLength(24)] public required string Name { get; set; }

    public List<Model> Models { get; set; } = [];

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