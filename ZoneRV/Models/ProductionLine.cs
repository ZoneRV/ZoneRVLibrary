namespace ZoneRV.Models;

[DebuggerDisplay("{Name}")]
public class ProductionLine
{
    public required int Id { get; set; }
    public required string Name { get; set; }

    public List<Model> Models { get; set; } = [];

    public static bool operator ==(ProductionLine? first, ProductionLine? second)
        => (first is null && second is null) || 
           first is not null && 
           second is not null && 
           first.Id == second.Id;

    public static bool operator !=(ProductionLine? first, ProductionLine? second)
        => (first is not null && second is null) ||
           (first is null && second is not null) || 
           first is not null && 
           second is not null && 
           first.Id != second.Id;
}