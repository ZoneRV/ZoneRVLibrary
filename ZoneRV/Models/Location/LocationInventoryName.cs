using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZoneRV.Models.Location;

[Table("LocationInventoryName")]
public class LocationInventoryName
{
    [Key, Required]
    public int Id { get; set; }
    
    [Required]
    public required string ServiceType { get; set; }
    
    [Required]
    public required string CustomName { get; set; }
    
    public required ProductionLine Line { get; set; }
}