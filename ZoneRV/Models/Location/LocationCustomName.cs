using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZoneRV.Models.Location;

/// <summary>
/// Represents a customized name for a specific service type in a production line or location.
/// </summary>
/// <remarks>
/// This class is used to define custom names for a service type and associates it with a production line.
/// It aids in identifying services with user-defined names within a production system.
/// </remarks>
public class LocationCustomName
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
    
    [ForeignKey("LocationId")] public required Location Location { get; set; }
}