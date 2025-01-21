using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace ZoneRV.Services.Trello.Models;

public class VanId
{
    
    [MaxLength(24)]                public          string? Id      { get; set; }
    [Key, Required, MaxLength(24)] public required string  VanName { get; init; }
                                   public          bool    Blocked { get; set; } = false;
    [MaxLength(1024)]              public          string? Url     { get; set; }
}