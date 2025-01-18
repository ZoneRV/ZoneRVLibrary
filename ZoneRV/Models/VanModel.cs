namespace ZoneRV.Models;

[DebuggerDisplay("{Name} - {ProductionLine.Name}")]
public class VanModel
{
    public required int Id { get; set; }
    public required int LineId { get; set; }
    public required ProductionLine ProductionLine { get; set; }
    public required string Name { get; set; }
    public required string? Description { get; set; }
    public required string Prefix { get; set; }
}