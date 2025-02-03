using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZoneRV.Models.Location;


[Table("LineLocation")]
public class LineLocation
{
    [Key, Required] public int Id { get; set; }
    
    [Required] public required ProductionLine    Line              { get; set; }
    [Required] public required WorkspaceLocation WorkspaceLocation { get; set; }
    
    [JsonIgnore] public virtual ICollection<LocationInventoryName>  InventoryLocations  { get; set; } = default!;
    [JsonIgnore] public virtual ICollection<LineLocationCustomName> CustomLocationNames { get; set; } = default!;
    
    /// <summary>
    ///Use <see cref="LocationFactory"/>  to create locations
    /// </summary>
    internal LineLocation()
    {
        
    }
}