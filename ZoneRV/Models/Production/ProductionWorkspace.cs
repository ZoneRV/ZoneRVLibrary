using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZoneRV.Models.Production;

[Table("Workspace")]
public class ProductionWorkspace
{
    [Key]
    public int Id { get; set; }
    
    [Required, MaxLength(128)]
    public required string Name        { get; set; }
    
    [MaxLength(1024)]
    public string? Description { get; set; }
    
    public virtual ICollection<ProductionLine> Lines { get; set; } = default!;
    public virtual ICollection<WorkspaceLocation> WorkspaceLocations { get; set; } = default!;
}