using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZoneRV.Models.Van;

[DebuggerDisplay("{Name}")]
[Table("AreaOfOrigin")]
public class AreaOfOrigin
{
    [Key, Required] public required int Id { get; set; }
    [MaxLength(24)] public required string Name { get; set; }
}