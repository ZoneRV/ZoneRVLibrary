using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZoneRV.Models.Location;

public class LineLocationCustomName
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
    
    [Column(TypeName = "decimal(18,2)"), Required] 
    public decimal Order { get; set; }
    
    [ForeignKey("LineLocationId"), Required] public required LineLocation LineLocation { get; set; }
}