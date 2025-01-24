using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZoneRV.Models.Location;

/// <summary>
/// Represents the inventory name details associated with a location.
/// </summary>
/// <remarks>
/// This class is used to define custom names for a service type and associates it with a production line.
/// It aids in identifying services with user-defined names within a production system.
/// </remarks>
[Table("LocationInventoryName")]
public class LocationInventoryName
{
    [Key, Required]
    public int Id { get; set; }
    
    /// <summary>
    /// Gets or sets the Service Type associated with the location.
    /// </summary>
    [Required, MaxLength(24)]
    public required string ServiceType { get; set; }
    
    [Required, MaxLength(128)]
    public required string CustomName { get; set; }
    
    public required ProductionLine Line { get; set; }
}