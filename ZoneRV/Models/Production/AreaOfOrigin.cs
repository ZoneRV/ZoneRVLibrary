﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ZoneRV.Serialization;

namespace ZoneRV.Models.Production;

/// <summary>
/// Represents the area of origin associated with a production process or entity.
/// </summary>
[DebuggerDisplay("{Name}:{Line.Name}")]
[Table("AreaOfOrigin")]
public class AreaOfOrigin
{
    [Key, Required] public          int     Id   { get; set; }
    [MaxLength(24)] public required string  Name { get; set; }
    [MaxLength(24)] public          string? Icon { get; set; }
    
    [ForeignKey("LineId"), Required, OptionalJsonField] 
    public required ProductionLine Line { get; set; }
}