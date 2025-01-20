using System.ComponentModel.DataAnnotations;

namespace ZoneRV.Models.Location;

public class LocationCustomName
{
    [Key, Required]
    public int Id { get; set; }
    
    [Required]
    public required string ServiceType { get; set; }
    
    [Required]
    public required string CustomName { get; set; }
    
    public required ProductionLine Line { get; set; }
}