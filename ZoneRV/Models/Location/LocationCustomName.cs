using System.ComponentModel.DataAnnotations;

namespace ZoneRV.Models.Location;

public class LocationCustomName
{
    [Key, Required]
    public int Id { get; set; }
    
    [Required, MaxLength(24)]
    public required string ServiceType { get; set; }
    
    [Required, MaxLength(128)]
    public required string CustomName { get; set; }
    
    public required ProductionLine Line { get; set; }
}