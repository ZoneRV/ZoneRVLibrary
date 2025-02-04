using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZoneRV.Models.Location;


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
    
    [ForeignKey("LocationId"), Required] public required OrderedLineLocation LineLocation { get; set; }
}